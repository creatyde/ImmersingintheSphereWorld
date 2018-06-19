using System.Collections.Generic;
using UnityEngine;

public class Spheres : MonoBehaviour {

    // rendering prefabs
    public GameObject prefab;
    public GameObject prefabInv;
    public GameObject prefabTest;

    //panoramic materials
    public Material panoOut;

    // colors
    Color color1 = new Color(135, 139, 114, 12);
    Color color2 = new Color(244, 70, 120, 12);

    // sound prefabs
    public GameObject aroundSound; // in personWorld, outside of viewer
    public GameObject insideSound; // inside of viewer
    public GameObject outsideSound; // in root, outside of personWorld
    public GameObject encasingSound; // personWorld, root, --> 2D / flat

    // rendering
    public int N; // maximum number of balls available for rendering
    private float hiResThreshold; // if size bigger than this, use better ball
    GameObject[] renderBalls;
    GameObject[] renderBalls2; // for inside
    float largeRadius = 200;

    // other
    public int recursionDepth = 1; // how many levels of balls in balls are seen, >= 1
    private float globalTime = 0;

    // person world
    public GameObject person;
    public GameObject camera; // to be moved in relation to the person based on size...
    private Sphere viewer;
    private Sphere root; // root sphere
    private Sphere personWorld; // sphere viewer is in
    private GameObject pWBall;
    private GameObject personBall;

    // sound instances
    public AudioSource enterWorld;
    public AudioSource leaveWorld;
    public AudioSource enterInternal;
    public AudioSource leaveInternal;


    // indexing
    private List<int> IndexMap;
    private int endMarker;

    // size accepted
    private float sizeMin = 0.03f; 
    private float sizeMax = 0.2f; 

    // get size of viewer (local)
    public float GetViewerSize()
    {
        return viewer.radius;
    }
    // get size of world
    public float GetWorldSize() {
        return personWorld.worldRadius;
    }

    // check if a point is within a sphere (in local frame)
    bool WithinLocal(Vector3 point, Sphere S) {
        return (S.position - point).magnitude <= S.radius;
    }

    // check if a point is within a sphere (in global frame)
    bool WithinGlobal(Vector3 point, Sphere S) {
        return (S.worldPosition - point).magnitude <= S.worldRadius;
    }

    // check if a point is significantly within a sphere (in global frame)
    bool WithinGlobalError(Vector3 point, Sphere S)
    {
        return (S.worldPosition - point).magnitude <= S.worldRadius * 0.95f; // more clearly within, "prevents flickering in diagonal"
    }

    // check if two balls are equal
    bool Equals(Sphere S1, Sphere S2) {
        return S1 == S2;
    }

    bool IsChild(Sphere S, Sphere T) {
        for (int i = 0; i < T.numChildren; i++) {
            if (Equals(S, T.children[i])) return true;
        }
        return false;
    }

    bool IsParent(Sphere T, Sphere S) {
        return Equals(S.parent, T);
    }

    // make T parent of S
    void MakeParent(Sphere S, Sphere T) {
        S.parent = T;
        // use current world radius/position
        S.radius = S.worldRadius / T.worldRadius;
        S.position = (S.worldPosition - T.worldPosition) / T.worldRadius;
    }
    // make S child of T
    void MakeChild(Sphere T, Sphere S) {
        T.children.Add(S);
        T.numChildren += 1;
        T.numDescendents += 1 + S.numDescendents;
    }

    // S is child, T is parent
    void MakeRelationship(Sphere S, Sphere T)
    {
        MakeParent(S, T);
        MakeChild(T, S);
    }

    // remove S as T's child
    void RemoveRelationship(Sphere S, Sphere T)
    {
        T.numChildren -= 1;
        T.numDescendents -= 1 + S.numDescendents;
        T.children.Remove(S); // remove reference
        S.parent = null; // so no longer T
    }

    // find appropriate scale bound of new potential child given peers
    float FindPeerScale(List<Sphere> C, Vector3 initPos, float initScaleThres) {
        float currScale = initScaleThres;
        float calc;
        for (int i = 0; i < C.Count; i++)
        { 
            calc = (C[i].position - initPos).magnitude - C[i].radius;
            if (calc < currScale) {
                currScale = calc;
                if (currScale < 0) return -1; // automatically bad, position within peer
            }
        }

        return currScale;
    }
    
    // create abstract children from any arbitrary ball
    int GenerateChildren(Sphere S, int available, int currDepth) {
        // RETURN
        if (currDepth == 0) return S.numDescendents;

        // generate number of children
        int maximum = (int)Mathf.Pow(((float)available), 1.0f / ((float)currDepth));
        int toMake = maximum;
        int childMax = (int)((float)available / (float)toMake) - 1; // -1 for each made child of current

        int index = S.numChildren;
        Vector3 initPos; float initRadius;
        // children parameters
        S.numChildren += toMake; // S.numChildren is set before
        int countDesc = toMake;
        // RETURN
        if (toMake == 0) return S.numDescendents; // no more children to make

        for (int i = 0; i < toMake; i++) {
            // initialize position/radius
            initRadius = -1;
            initPos = Vector3.zero;
            // obtain valid position, scale
            while (initRadius < sizeMin) {
                initPos = Random.insideUnitSphere;
                // use existing children to find new radius/position
                initRadius = FindPeerScale(S.children, initPos, 1 - initPos.magnitude); // bounded by parent surface
            }
            initRadius *= 0.75f; // scale factor
            S.children.Add(new Sphere());
            (S.children[index]).parent = S;
            (S.children[index]).position = initPos;
            (S.children[index]).worldPosition = (S.children[i]).position * S.worldRadius + S.worldPosition;
            (S.children[index]).radius = initRadius;
            (S.children[index]).worldRadius = (S.children[i]).radius * S.worldRadius;
            (S.children[index]).children = new List<Sphere>();
            (S.children[index]).numChildren = 0;
            (S.children[index]).numDescendents = 0;
            (S.children[index]).rotation = Random.Range(0, 360); // around y-axis

            // continuation
            if (currDepth > 0 && childMax > 0)
                countDesc += GenerateChildren(S.children[index], childMax, currDepth - 1);

            index++; // update index
        }

        // of num descendants
        S.numDescendents += countDesc;
        return S.numDescendents;
    }


    // give first available index, and also manipulate rotation of ball upon assign
    void AssignIndex(Sphere S)
    {
        // ASSUMPTION endMarker < N
        if (endMarker >= N) Debug.Log("AssignIndex: endMarker is out of bounds " + endMarker);
        if (endMarker < 0) Debug.Log("AssignIndex: endMarker is negative");
        Debug.Log(endMarker);
        S.drawIndex = IndexMap[endMarker]; // updated endMarker 
        endMarker++;
    }

    // assigns index to current ball and its children (for initialization or updating)
    void AssignIndices(Sphere S)
    {
        // ASSUMPTION index + S.numDescendents < N, so ALL of S and descendants have index
        if (endMarker + S.numDescendents >= N) Debug.Log("AssignIndices: not enough indices for sphere set");
        if (!Equals(S, viewer))
            AssignIndex(S);
        for (int i = 0; i < S.numChildren; i++)
        {
            AssignIndices(S.children[i]);
        }
    }

    // collect indices in current S
    List<int> CollectIndices(Sphere S) {
        List<int> collectIndices = new List<int>();
        // self, if not person
        if (!Equals(S, viewer))
            collectIndices.Add(S.drawIndex);
        // look within children
        for (int i = 0; i < S.numChildren; i++) {
            collectIndices.AddRange( CollectIndices(S.children[i]) ); // add current list to original
        }

        return collectIndices;
    }

    // removes indices (usually from variant of CollectIndices)
    void DestroyByIndices(List<int> collectIndices)
    {
        // delete all indices from list, hide drawn balls
        int trueIndex;
        int childCount;
        GameObject child;
        for (int i = 0; i < collectIndices.Count; i++) {
            // get index
            trueIndex = collectIndices[i];
            
            // ASSUMPTION trueIndex is in IndexMap at index < endMarker

            if (trueIndex == -1) continue;

            // remove sound (also, should not remove person...)
            childCount = renderBalls[trueIndex].transform.childCount;
            for (int j = 0; j < childCount; j++) {
                child = renderBalls[trueIndex].transform.GetChild(j).gameObject;
                if (child.GetComponent<AudioSource>() != null) // component has audio
                    Destroy(child);
            }

            // remove index in use from current position
            IndexMap.Remove(trueIndex);
            endMarker -= 1; // edit endMarker

            // hide ball
            renderBalls[trueIndex].GetComponent<Renderer>().enabled = false;
            renderBalls[trueIndex].transform.GetChild(0).GetComponent<Renderer>().enabled = false;

        }
        // add indices to END of list
        for (int i = 0; i < collectIndices.Count; i++)
        {
            trueIndex = collectIndices[i];
            if (trueIndex == -1) continue;
            IndexMap.Add(trueIndex);
        }
    }

    // remove smallest ball from a ball such as root
    // guaranteed leaf
    // ASSUMPTION, there exists a valid smallest sphere, despite the disclusion of the viewer
    // can check within viewer for smallest balls
    Sphere FindSmallestSphere(Sphere S) {
        // return the self if a leaf
        Sphere smallestSphere = S;
        if (S.numChildren == 0)
            return S;

        // else, look at children
        float smallestRadius = S.worldRadius;
        Sphere tempSmall;
        for (int i = 0; i < S.numChildren; i++) {
            tempSmall = FindSmallestSphere(S.children[i]);
            if (Equals(tempSmall, viewer) || tempSmall == null) continue;
            if (tempSmall.worldRadius < smallestRadius) {
                smallestRadius = tempSmall.worldRadius;
                smallestSphere = tempSmall;
            }
        }
        
        if (Equals(smallestSphere, S)) return null; // if no non-viewer children, so only viewer as child
        return smallestSphere;
    }

    // find small balls from a ball
    List<Sphere> FindSmallSpheres(Sphere S, float threshold) {
        List<Sphere> small = new List<Sphere>();
        if (Equals(S, viewer)) {
            return small; // do not delete S or anything inside
        }

        // is small (so children are small, as assumed)
        if (S.worldRadius < threshold) {
            small.Add(S);
            return small;
        }
        // not small, create list of small
        for (int i = 0; i < S.numChildren; i++) {
            small.AddRange(FindSmallSpheres(S.children[i], threshold));
        }

        return small;
    }

    // remove smallest sphere within current (guaranteed removal, as must exist a leaf)
    void RemoveSmallestSphere(Sphere S) {
        // smallest sphere, a leaf
        Sphere T = FindSmallestSphere(S);
        // ASSUMPTION, T is not null
        if (T == null) Debug.Log("RemoveSmallestSphere: no valid smallest sphere");
        // ASSUMPTION, T is not viewer
        if (Equals(T, viewer)) Debug.Log("RemoveSmallestSphere: T is viewer");
        // ASSUMPTION, T is not world
        if (Equals(T, personWorld)) Debug.Log("RemoveSmallestSphere: T is personWorld");
        // ASSUMPTION, T is not root
        if (Equals(T, root)) Debug.Log("RemoveSmallestSphere: T is root");
        // ASSUMPTION T has a parent
        if (T.parent == null) Debug.Log("RemoveSmallestSphere: T has no parent " + T.drawIndex);

        // remove reference
        RemoveRelationship(T, T.parent);

        DestroyByIndices(new List<int>(new int[] { T.drawIndex } ) );
    }

    // remove spheres under size threshold (not guaranteed removal)
    void RemoveSmallSpheres(Sphere S, float threshold)
    {
        // small spheres
        List<Sphere> TList = FindSmallSpheres(S, threshold);

        // collect indices from all small spheres
        List<int> collectIndices = new List<int>();
        for (int i = 0; i < TList.Count; i++) {
            if (Equals(TList[i], viewer)) continue; // cannot remove viewer

            collectIndices.AddRange(CollectIndices(TList[i]));

            // ASSUMPTION TList[i] is not personWorld
            if (Equals(TList[i], personWorld)) Debug.Log("RemoveSmallSpheres: tried to remove personWorld");
            // ASSUMPTION Tlist[i] is not root
            if (Equals(TList[i], root)) Debug.Log("RemoveSmallSpheres: tried to remove root");
            // ASSUMPTION TList[i] has parent
            if (TList[i].parent == null) Debug.Log("RemoveSmallSpheres: sphere to be removed has no parent");

            RemoveRelationship(TList[i], TList[i].parent);
        }

        // delete only indices in collectIndices
        DestroyByIndices(collectIndices);
    }

    // delete old root, and all children besides T, which is new root
    void RemoveHighestEchelon(Sphere T) {
        // ASSUMPTION T is child of root
        if (!Equals(T.parent, root)) Debug.Log("RemoveHighestEchelon: T's parent is not root");

        // collect indices, and all previous references to them will be lost anyway
        List<int> collectIndices = new List<int>();
        collectIndices.Add(root.drawIndex); // from root

        // from root's children (besides T)
        for (int i = 0; i < root.numChildren; i++)
        {
            if (Equals(root.children[i], T)) continue;
            collectIndices.AddRange(CollectIndices(root.children[i]));
        }
        DestroyByIndices(collectIndices);

        // set new root
        root = T;
        root.parent = null;
        root.worldRadius = largeRadius;
        root.worldPosition = Vector3.zero;
    }

    // create root, that is parent of old root
    void CreateNewRoot() {
        Sphere S = root; // save, as will modify "root" later

        // create rootNew
        Sphere rootNew = new Sphere();
        // make sure endMarker is valid
        if (endMarker == N) { RemoveSmallestSphere(S); }
        
        AssignIndex(rootNew);
        rootNew.parent = null;
        rootNew.position = Vector3.zero;
        rootNew.radius = 1;
        rootNew.worldPosition = Vector3.zero;
        // world radius now (for relationship to S purposes)
        rootNew.worldRadius = ((S.worldPosition - rootNew.worldPosition).magnitude + S.worldRadius) * 3f; 
        rootNew.children = new List<Sphere>();
        rootNew.numChildren = 0;
        rootNew.numDescendents = 0;
        rootNew.rotation = Random.Range(0, 360);

        MakeRelationship(S, rootNew);

        root = rootNew;
        root.worldRadius = largeRadius; // real world radius
    }

    // update personWorld
    void UpdateViewerWorld(Sphere T) {
        personWorld = T;
        // ASSUMPTION T is already in relationship with root
        if (!IsParent(root, personWorld)) Debug.Log("UpdateViewerWorld: T's parent is not root");
        if (!IsChild(personWorld, root)) Debug.Log("UpdateViewerWorld: T is not child of root");
        person.transform.SetParent(renderBalls[personWorld.drawIndex].transform, false); // player ball, or could just be pW...?

    }

    // give S additional children/descendents
    void UpdateChildren(Sphere S) {
        int haveNum = S.numChildren;
        // generate children, has to apply to S!!!
        GenerateChildren(S, N - endMarker, recursionDepth);
        // assign indices to person world's descendents that have not been addressed
        for (int i = haveNum; i < S.numChildren; i++)
        {
            AssignIndices(S.children[i]); // update endMarker
        }
    }

 
    // peer to peer intersection, when position of S is within T
    void SphereEnterSphere(Sphere S, Sphere T) {
        // ASSUMPTION, S and T have the same parent
        if (!Equals(S.parent, T.parent))  Debug.Log("SphereEnterSphere: S and T are not peers\n" + 
            S.drawIndex + " " + T.drawIndex + " " + S.parent.drawIndex + " " + T.parent.drawIndex);
        // ASSUMPTION S is within T
        if (!WithinGlobal(S.worldPosition, T)) Debug.Log("SphereEnterSphere: S not in T " 
            + (T.worldPosition - S.worldPosition).magnitude / T.worldRadius);

        // destroy relationship
        if (S.parent == null) Debug.Log("SphereEnterSphere: S does not have parent");
        RemoveRelationship(S, S.parent);
        
        if (Equals(S, viewer)) {
            Debug.Log("SphereEnterSphere: new world");
            RemoveHighestEchelon(personWorld); // personWorld is new root
            // ASSUMPTION, already relationship between T and personWorld, as viewer's parent is personWorld
            MakeRelationship(viewer, T);
            UpdateViewerWorld(T); // T -> personWorld
            UpdateWorld(root); // fix all worldPositions
            // cull small spheres
            RemoveSmallSpheres(personWorld, 0.05f * personWorld.worldRadius);

            // play enterWorld
            enterWorld.Play();
        }
        else if (Equals(S, personWorld)) {// fix if personWorld
            Debug.Log("SphereEnterSphere: personWorld entering peer");
            RemoveHighestEchelon(T); // T is new root
            MakeRelationship(S, T); // make child/parent relationship
            UpdateWorld(root);
        }
        else {
            // create relationship, child S and parent T
            MakeRelationship(S, T);
            
            if (Equals(T, viewer)) {
                // play enterInternal
                enterInternal.Play();
            }
        }
    }

    // peer to parent-peer intersection, when position of S is within T
    // because requires existence of grandparent of S (and parent of T), cannot have personWorld as S
    void SphereEnterSphereP(Sphere S, Sphere T)
    {
        // ASSUMPTION S and T are neither a descendent of the other, nor reside in the same world
        // ASSUMPTION S is within T
        if (!WithinGlobal(S.worldPosition, T)) Debug.Log("SphereEnterSphereP: S not in T "
            + (T.worldPosition - S.worldPosition).magnitude / T.worldRadius);

        // destroy relationship
        if (S.parent == null) Debug.Log("SphereEnterSphere: S does not have parent");
        RemoveRelationship(S, S.parent);

        if (Equals(S, viewer))
        {
            Debug.Log("SphereEnterSphere: new world");
            // keep old root
            // make relationship
            MakeRelationship(viewer, T);
            UpdateViewerWorld(T); // T -> personWorld
            UpdateWorld(root); // fix all worldPositions
            // cull small spheres
            RemoveSmallSpheres(personWorld, 0.05f * personWorld.worldRadius);

            // play enterWorld
            enterWorld.Play();
        }
        else
        {
            // create relationship, child S and parent T
            MakeRelationship(S, T);

            if (Equals(T, viewer))
            {
                enterInternal.Play();
            }
        }
    }

    // child to parent intersection, when position of S outside T
    void SphereLeaveSphere(Sphere S, Sphere T)
    {
        // ASSUMPTION, T is S's parent
        if (!IsParent(T,S)) Debug.Log("SphereLeaveSphere: T is not S's parent");
        // ASSUMPTION, S is outside of T
        if (WithinGlobal(S.worldPosition, T)) Debug.Log("SphereEnterSphere: S is still in T "
            + (T.worldPosition - S.worldPosition).magnitude / T.worldRadius);

        
        if (Equals(S, viewer)) {
            // destroy relationship
            RemoveRelationship(S, T);

            Debug.Log("SphereLeaveSphere: leaving world");
            // ASSUMPTION, T = personWorld
            if (!Equals(T, personWorld)) Debug.Log("SphereLeaveSphere: T is not personWorld");

            // what if S leaves not only world, but also root? need to do CreateNewRoot twice
            if (!WithinGlobal(viewer.worldPosition, root))
            { // check that S position is not in root
                Debug.Log("SphereLeaveSphere: viewer extreme case, leaving world and root");
                CreateNewRoot(); // -> first level new root
                Sphere newPW = root; // save first level root
                CreateNewRoot(); // -> second level new root
                // ASSUMPTION, already relationship between newPW and root
                MakeRelationship(viewer, newPW);
                UpdateViewerWorld(newPW);
            }
            else
            {
                CreateNewRoot();
                MakeRelationship(viewer, T.parent);
                UpdateViewerWorld(T.parent);  // T.parent -> personWorld
            }
            UpdateWorld(root); // update world
            // cull small spheres
            RemoveSmallSpheres(root, 0.05f * personWorld.worldRadius);

            // play leaveWorld
            leaveWorld.Play();
        }
        else if (Equals(S, personWorld)) {
            // DO NOTHING
        }
        else if (Equals(T, root)) {
            // DO NOTHING
        }
        else
        {
            // destroy relationship
            RemoveRelationship(S, T);

            // create relationship
            Sphere P = T.parent;
            MakeRelationship(S, P);

            // if (T.isPerson) {
            if (Equals(T, viewer)) {
                // play leaveInternal
                leaveInternal.Play();
            }
        }
    }

    // dist = 1, 0 : dist = 0, 1, need something more dramatic
    float DistFunc(float x) { return Mathf.Pow(1-x, 5); }
    
    // give new local scale
    void LocalScaling(Sphere S) {
        if (Equals(S, root)) return; // no change

        Vector3 position = S.position;
        float radius = S.radius;

        // by parent ball, growing
        float dist = 1 - (position.magnitude + radius); // how close boundary is to other
        // how much normalized vector to surface aligns with movement
        float directionWeight = Vector3.Dot(S.movement, position.normalized);

        // ASSUMPTION dist < 1
        float parentInfluence = (0.5f - radius) * DistFunc(dist) * directionWeight; 

        // influence by peer balls
        List<Sphere> peers = S.parent.children;
        int peerNum = S.parent.numChildren;

        float peerInfluence = 0;
        Vector3 diff;  Sphere temp;
        for (int i = 0; i < peerNum; i++) {
            temp = peers[i];
            if (Equals(temp, S)) continue; // do not compare with the self

            diff = temp.position - position;
            dist = diff.magnitude - temp.radius;
            directionWeight = Vector3.Dot(S.movement, diff.normalized);

            // ASSUMPTION, dist between peer spheres is positive
            if (radius < temp.radius)
            { // only change if already smaller
                if (Equals(temp, viewer))
                    peerInfluence += (0.5f * temp.radius - radius) * directionWeight * DistFunc(dist / (2 - temp.radius));
                else
                    peerInfluence += (0.5f * temp.radius - radius) * directionWeight * DistFunc(dist / (2 - temp.radius));
            }
        }

        float influence = parentInfluence + peerInfluence; 

        float potScale = radius + influence * Time.deltaTime;
        if (potScale > radius * 1.01f) potScale = radius * 1.01f; // upper bound 
        if (potScale < radius * 0.99f) potScale = radius * 0.99f; // lower bound 
        
        if (potScale > sizeMax || potScale < sizeMin) { // potScale out of bounds
            // see if can improve S.radius if needed
            if (radius > sizeMax)
                radius += (sizeMax - radius) * Time.deltaTime; // downSize
            if (radius < sizeMin)
                radius += (sizeMin - radius) * Time.deltaTime; // upSize
        }
        else {
            radius = potScale;
        }
        
       S.radius = radius;
    }

    // new local movement
    void LocalMovement(Sphere S) {
        // if (S.isPerson) {
        if (Equals(S, viewer)) {
            S.movement = Vector3.zero;
            return;
        }
        if (Equals(S, root)) return; // no change

        Vector3 oldMovement = S.movement;
        Vector3 leave = S.position.normalized - S.position;

        List<Sphere> peers = S.parent.children;
        int peerNum = S.parent.numChildren;
        Vector3 affect = Vector3.zero;
        Vector3 distVec; float distMag; Vector3 distNorm;
        float invDist; float sizeRatio; float sizeNorm; float sizeWeight;
        Sphere temp;
        for (int i = 0; i < peerNum; i++) {
            temp = peers[i];
            if (Equals(temp, S)) continue; // do not compare with the self

            distVec = temp.position - S.position;
            distNorm = distVec.normalized;
            distVec -= distNorm * (S.radius + temp.radius); // distance btwn boundary
            distMag = distVec.magnitude; // 2 is the max theoretically
            invDist = 1 - distMag / 2;

            sizeRatio = temp.radius / S.radius;

            sizeNorm = (sizeRatio - 1) / (sizeRatio + 0.5f); 
            sizeWeight = 1.5f * sizeNorm * sizeNorm - 0.075f;
            
            affect += distVec * invDist * invDist * sizeWeight;
        }

        // higher leave factor
        Vector3 influence =  affect + 0.025f * leave; 
        influence /= 25f; // make smaller

        // check interpolation
        Vector3 difference = influence - oldMovement;
        float diffMag = difference.magnitude;
        float threshold = (1 - S.position.magnitude) / 10f; // less differentiation closer to surface, more decisiveness

        if (diffMag > threshold) {
            influence = oldMovement + difference * threshold / difference.magnitude;
        }

        S.movement = influence;
    }

    // local position/radius
    // here, relationships between objects is constant (same parent/children/peer)
    void UpdateLocal(Sphere S)
    {
        // update local scale first, as depends on previous frame's parameters
        LocalScaling(S);
        LocalMovement(S);

        // update local position
        S.position += S.movement * Time.deltaTime;

        // make sure root does not change position
        if (Equals(S, root)) S.position = Vector3.zero;

        // recurse
        for (int i = 0; i < S.numChildren; i++) {
            UpdateLocal(S.children[i]);
        }
    }
    
    // update true world position
    void UpdateWorld(Sphere S) {
        // by this time, parent has been updated
        Sphere temp;
        float check;
        for (int i = 0; i < S.numChildren; i++) {
            temp = S.children[i];
            temp.worldPosition = S.worldPosition + temp.position * S.worldRadius;
            check = temp.worldRadius;
            temp.worldRadius = S.worldRadius * temp.radius;
            UpdateWorld(temp);
        }
    }

    // find parent to children interactions
    bool VerticalIntersections(Sphere S) {

        bool inter = false;
        Sphere temp;
        for (int i = 0; i < S.numChildren; i++) {
            temp = S.children[i];
            inter = inter || VerticalIntersections(temp);
            if (temp.position.magnitude > 1) {
                inter = true;
                SphereLeaveSphere(temp, S);
            }
        }
        return inter;
    }

    struct SizeTuple {
        public float rad;
        public Sphere S;
    };

    // create list of current and all children
    public List<Sphere> GetAncestry(Sphere S) {
        List<Sphere> list = new List<Sphere>();
        for (int i = 0; i < S.numChildren; i++) {
            list.AddRange(GetAncestry(S.children[i]));
        }
        return list;
    }

    // find diagonal interactions
    // top to bottom, a peer prefers entering the smallest peer that is larger than itself
    // look from smallest to largest spheres, so smallest spheres enter first
    bool DiagonalIntersections(Sphere S)
    {
        bool inter = false;
        
        // bottom to top, examine children first
        for (int i = 0; i < S.numChildren; i++)
        {
            inter = inter || DiagonalIntersections(S.children[i]);
        }

        // PEERS (with possible updates in internal structure)
        List<Sphere> list = S.children;

        Sphere T = null;
        if (S.parent == null) return inter; // no need to check further
        T = S.parent;

        // PARENT PEERS + CHILDREN
        List<Sphere> baseList = new List<Sphere>();
        for (int i = 0; i < T.numChildren; i++) {
            if (Equals(T.children[i], S)) continue; // no including the self!!!
            baseList.AddRange(GetAncestry(T.children[i]));
        }

        // making list of struct tuples
        List<SizeTuple> list2 = new List<SizeTuple>();
        SizeTuple temp;
        for (int i = 0; i < baseList.Count; i++)
        {
            temp = new SizeTuple();
            temp.S = baseList[i];
            temp.rad = temp.S.worldRadius; // real radius, for global comp
            list2.Add(temp);
        }
        // sorting list https://stackoverflow.com/questions/4668525/sort-listtupleint-int-in-place
        list2.Sort((x, y) => x.rad.CompareTo(y.rad)); // sort by size in ascending order

        
        Sphere S1; Sphere S2;
        for (int j = 0; j < list.Count; j++) {
            S1 = list[j];
            S2 = null;
            // check if current sphere is within any diagonal sphere of SMALLEST size, break at first sphere
            for (int k = 0; k < list2.Count; k++)
            {
                S2 = list2[k].S;
                // must have S1 be smaller and clearly within S2
                if (S1.worldRadius < S2.worldRadius && WithinGlobalError(S1.worldPosition, S2)) // world position!!!
                {
                    SphereEnterSphereP(S1, S2);
                    break;
                }
            }
        }

        return inter;
    }


    // find peer to peer interactions
    // top to bottom, a peer prefers entering the smallest peer that is larger than itself
    // look from smallest to largest spheres, so smallest spheres enter first
    bool HorizontalIntersections(Sphere S) {
        bool inter = false;

        // making list of struct tuples
        List<SizeTuple> list = new List<SizeTuple>();
        SizeTuple temp;
        for (int i = 0; i < S.numChildren; i++) {
            temp = new SizeTuple();
            temp.S = S.children[i];
            temp.rad = temp.S.radius;
            list.Add(temp);
        }
        // sorting list https://stackoverflow.com/questions/4668525/sort-listtupleint-int-in-place
        list.Sort((x, y) => x.rad.CompareTo(y.rad)); // sort by size in ascending order

        int index = 0;
        Sphere S1; Sphere S2; Sphere tempS;
        while (index < list.Count) { // dynamic list, loop will terminate by index or by size decreasing
            // ASSUMPTION, list.Count = S.numChildren
            S1 = list[index].S;
            S2 = null;
            // check if current sphere is within any sphere of larger size, break at first sphere
            for (int i = index + 1; i < list.Count; i++) {
                tempS = list[i].S;
                if (WithinGlobal(S1.worldPosition, tempS)) {// if (WithinLocal(S1.position, tempS)) {
                    S2 = tempS;
                    break;
                }
            }
            if (S2 == null) index++; // move to next sphere
            else {
                // inter occured
                inter = true; //occured

                // remove current from the list so list.Count--
                list.RemoveAt(index);
                SphereEnterSphere(S1, S2);
            } 
        }

        // by this point, S's parameters have been fixed
        for (int i = 0; i < S.numChildren; i++) {
            inter = inter || HorizontalIntersections(S.children[i]);
        }

        return inter;
    }

    // update frame
    bool UpdateSpheres(Sphere S) {
        // update local position, scale first
        UpdateLocal(S);

        //update world position scale
        UpdateWorld(S);

        // check for intersections
        bool hi = HorizontalIntersections(S); 
        //bool si = DiagonalIntersections(S); 
        bool vi = VerticalIntersections(S);

        return (vi || hi); // si 
    }

    // print int list
    string PrintList(List<int> list) {
        string s = "";
        for (int i = 0; i < list.Count; i++) {
            s += list[i].ToString() + " ";
        }
        return s;
    }

    // draw relationship tree
    string DrawRelationship(Sphere S, int calls) {
        string s = "";
        string t = "";
        for (int i = 0; i < calls; i++) {
            t += "\t";
        }
        if (Equals(S, root)) s += t + S.drawIndex + ":" + "root" + "\n";
        else s += t + S.drawIndex + ":" + S.parent.drawIndex + "\n";
        for (int i = 0; i < S.numChildren; i++) {
            s += DrawRelationship(S.children[i], calls+1);
        }
        return s;
    }

    void TurnRender(Sphere S) {
        if (!Equals(S, viewer))
        {
            renderBalls[S.drawIndex].GetComponent<Renderer>().enabled = true;
            renderBalls[S.drawIndex].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        }
        for (int i = 0; i < S.numChildren; i++) {
            TurnRender(S.children[i]);
        }
    }

    // type: around (0), inside (1), outside (2)
    // level "dist" from player
    void AudioManifest(Sphere S, int type, int level) {
        S.level = level;
        // below, only make change if needed
        if (!Equals(S, viewer))
        {
            if (type == 0)
            {
                if (S.sound == null)
                    S.sound = Instantiate(aroundSound, renderBalls[S.drawIndex].transform);
                else if (S.soundType != 0)
                { // change only if necessary
                    Destroy(S.sound); // make sure this is gone
                    S.sound = Instantiate(aroundSound, renderBalls[S.drawIndex].transform);
                }
                S.soundType = type;
            }
            else if (type == 1)
            {
                if (S.sound == null)
                    S.sound = Instantiate(insideSound, renderBalls[S.drawIndex].transform);
                else if (S.soundType != 1)
                {
                    Destroy(S.sound); // make sure this is gone
                    S.sound = Instantiate(insideSound, renderBalls[S.drawIndex].transform);
                }
                S.soundType = type;
            }
            else if (type == 2)
            {
                if (S.sound == null)
                    S.sound = Instantiate(outsideSound, renderBalls[S.drawIndex].transform);  // make sure this is gone
                else if (S.soundType != 2)
                {
                    Destroy(S.sound);
                    S.sound = Instantiate(outsideSound, renderBalls[S.drawIndex].transform);
                }
                S.soundType = type;
            }
        }
        for (int i = 0; i < S.numChildren; i++) {
            AudioManifest(S.children[i], type, level+1); // update distance
        }
    }

    // sound relationship update
    void AudioUpdate() {
        Sphere temp;

        // change personWorld and root manually
        personWorld.level = 0;
        if (personWorld.soundType != 3)
        {
            personWorld.soundType = 3;
            if (personWorld.sound != null) Destroy(personWorld.sound); // make sure this is gone
            personWorld.sound = Instantiate(encasingSound, renderBalls[personWorld.drawIndex].transform); // parenting here
        }
        root.level = 1;
        if (root.soundType != 3)
        {
            root.soundType = 3;
            if (root.sound != null) Destroy(root.sound); // make sure this is gone
            root.sound = Instantiate(encasingSound, renderBalls[root.drawIndex].transform); // parenting here
        }

        // around (by personWorld)
        for (int i = 0; i < personWorld.numChildren; i++)
        {
            temp = personWorld.children[i];
            if (Equals(temp, viewer)) continue;
            AudioManifest(temp, 0, 1);
        }
        // inside (by viewer)
        for (int i = 0; i < viewer.numChildren; i++)
        {
            temp = viewer.children[i];
            AudioManifest(temp, 1, 1);
        }

        // outside (by root)
        for (int i = 0; i < root.numChildren; i++)
        {
            temp = root.children[i];
            if (Equals(temp, personWorld)) continue;
            AudioManifest(temp, 2, 2);
        }
    }

    // adjust frequency accordingly
    void SoundBalls(Sphere S) {
        // if (!S.isPerson)
        if (!Equals(S, viewer))
        {
            // ASSUMPTION, S has a sound source
            if (S.sound == null) Debug.Log("SoundBalls: " + S.drawIndex + " has no sound source");
            else // only continue if have sound source
            {
                // decide pitch
                float pitch = Mathf.Sqrt(viewer.worldRadius / S.worldRadius); // pitch is higher when S is smaller, do it relative to viewer

                // decide volume, control using level
                float volume = 1;
                float dist = 1;
                if (S.soundType == 0 || S.soundType == 2)
                {
                    dist = (viewer.worldPosition - S.worldPosition).magnitude / (2 * personWorld.worldRadius); //around
                    // minimize dist effect a bit
                    volume = DistFunc(dist) / Mathf.Pow(2, S.level-1); // decreases when farther, do it relative to world

                }
                else if (S.soundType == 1)
                {
                    volume = 1 / Mathf.Pow(2, S.level-1);
                }

                // controller
                if (pitch > 20) { pitch = 20; }
                if (volume > 1) { volume = 1; }

                if (S.soundType != 3)
                {// if not world ball
                    S.sound.GetComponent<AudioSource>().pitch = pitch;
                    S.sound.GetComponent<AudioSource>().volume = volume;
                }
            }
        }
        for (int i = 0; i < S.numChildren; i++) {
            SoundBalls(S.children[i]);
        }
    }

    // draw/update from certain ball 
    void DrawBalls(Sphere S) {

        Vector3 size = new Vector3(S.worldRadius, S.worldRadius, S.worldRadius);
        GameObject R;

        // if (S.isPerson)
        if (Equals(S, viewer))
        {
            // here, using LOCAL parameters
            person.transform.localPosition = S.position;
            camera.transform.localPosition = Vector3.forward * S.radius * 0.8f; // now out as far as it should be
            float sizeP = S.radius;
            personBall.transform.localScale = new Vector3(sizeP, sizeP, sizeP); // local relative to MOVER

        }
        else if (Equals(S, personWorld))
        {
            int index = S.drawIndex;
            R = renderBalls[index];
            R.GetComponent<Renderer>().enabled = false;
            R.transform.GetChild(0).GetComponent<Renderer>().enabled = false; // inverted sphere should be first child
            R.transform.position = S.worldPosition;
            R.transform.localScale = size;
            R.transform.rotation = Quaternion.AngleAxis(S.rotation, Vector3.up);
            pWBall.transform.transform.position = S.worldPosition;
            pWBall.transform.localScale = size;
            pWBall.transform.rotation = Quaternion.AngleAxis(S.rotation, Vector3.up); // using draw1
        }
        else
        {
            int index = S.drawIndex;
            R = renderBalls[index];

            if (Equals(root, S))
            {
                R.GetComponent<Renderer>().enabled = false;
                R.transform.GetChild(0).GetComponent<Renderer>().enabled = false; // inverted sphere should be first child
                panoOut.SetFloat("Rotation", S.rotation);
            }
            else
            {
                // material -> by affect
                R.GetComponent<Renderer>().enabled = true;
                R.transform.GetChild(0).GetComponent<Renderer>().enabled = true; // inverted sphere should be first child
            }

            // transform
            R.transform.position = S.worldPosition;
            R.transform.localScale = size;
            R.transform.rotation = Quaternion.AngleAxis(S.rotation, Vector3.up); // rand rotation
        }
        // recurse
        for (int i = 0; i < S.numChildren; i++)
        {
            DrawBalls(S.children[i]);
        }
    }

    // Use this for initialization
    void Awake () {
        // renders
        pWBall = Instantiate(prefabInv);
        personBall = Instantiate(prefabTest, person.transform);

        renderBalls = new GameObject[N];
        for (int i = 0; i < N; i++) {
            renderBalls[i] = Instantiate(prefab);
            renderBalls[i].GetComponent<Renderer>().enabled = false; // true
            renderBalls[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false; // inverted sphere should be first child
        }

        // index mapping
        IndexMap = new List<int>();
        for (int i = 0; i < N; i++) {
            IndexMap.Add(i); // ith at i
        }
        
        // root -> personWorld
        root = new Sphere();
        root.seen = true; // for now
        root.worldPosition = Vector3.zero;
        root.worldRadius = largeRadius;
        root.parent = null;
        root.position = Vector3.zero; // magnitude <= 1, internal unit-sphere vector
        root.radius = 1;
        // root.isPerson = false;
        root.children = new List<Sphere>();
        root.numChildren = 0;
        root.numDescendents = 0;
        root.rotation = Random.Range(0, 360);
        personWorld = root; // save this
        
        // actual root
        CreateNewRoot(); // assigns a new index, which is discarded later

        // fix world 
        UpdateWorld(root);

        // generate balls from (new) root
        int ch = (int) (0.75f * N); // number of spheres in personWorld 
        GenerateChildren(personWorld, ch, recursionDepth); 
        GenerateChildren(root, N - ch - 3, recursionDepth); 

        // assign indices
        AssignIndices(root); // now fix endMarker properly

        // person
        viewer = new Sphere();
        viewer.drawIndex = -1; 
        viewer.seen = true;
        viewer.worldPosition = Vector3.zero;
        viewer.worldRadius = 0.15f * personWorld.worldRadius; 
        viewer.parent = personWorld;
        viewer.position = Vector3.zero;
        viewer.radius = 0.15f; 
        viewer.children = new List<Sphere>();
        viewer.numChildren = 0;
        viewer.numDescendents = 0;
        // viewer.rotation is irrelevant
        MakeRelationship(viewer, personWorld);
        person.transform.SetParent(renderBalls[personWorld.drawIndex].transform, false);

        // seen
        TurnRender(root);

        // sound
        AudioUpdate();

        // draw
        DrawBalls(root);
        
        // draw parenting tree
        Debug.Log(DrawRelationship(root, 0));
    }

    // maybe check if parent/children is symmetrical
    void HealthTest(Sphere S) {
        // make sure S is actually on, and that it has audio listener
        if (!Equals(S, viewer)) {
            Transform tr = renderBalls[S.drawIndex].transform;
            GameObject child;
            bool noSound = true;
            for (int j = 0; j < tr.childCount; j++)
            {
                child = tr.GetChild(j).gameObject;
                if (child.GetComponent<AudioSource>() != null) { // component has audio
                    noSound = false;
                    break;
                }
            }
            if (noSound) Debug.Log("HealthTest: " + S.drawIndex + " does not have AudioSource");
        }
        for (int i = 0; i < S.numChildren; i++)
        {
            HealthTest(S.children[i]);
        }
    }
	
	// Update is called once per frame
	void Update () {

        globalTime += Time.deltaTime;
        if (globalTime > 0) 
        {
            // update person's abstract position
            viewer.position = person.transform.localPosition;

            bool us = UpdateSpheres(root); // reposition
            hiResThreshold = personWorld.worldRadius * 0.25f; // hi res

            // sphere relationships changed, update what is seen
            if (us) {
                // do a generation again for ROOT after all has been taken care of
                UpdateChildren(personWorld); // root);

                // change visuals -> decide what is seen, decide what is rendered
                TurnRender(root);

                // change audio, also dictate relationship type/material used
                AudioUpdate();
            }

            // updated audio
            SoundBalls(root);

            // update everyone's sound (frequency)
            DrawBalls(root);
        }
    }
}

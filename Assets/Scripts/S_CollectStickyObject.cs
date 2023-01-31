using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(S_VolumeObject))]
public class S_CollectStickyObject : MonoBehaviour
{

    protected enum DistortType
    {
        DISTORT_TO_ORIGIN,
        DISTORT_GROUP_TO_ORIGIN,
        DISTORT_TO_FURTHEST_AWAY

    };

    [SerializeField] UnityEvent uiEvent;

    public float VolumeBias = 3;

    
    private MeshCollider meshCollider;
    private Mesh OriginalCollisionMesh;


    private void Start()
    {
        meshCollider = GetComponent<MeshCollider>();

        ;
        meshCollider.sharedMesh = DuplicateMesh(meshCollider.sharedMesh);
        OriginalCollisionMesh = meshCollider.sharedMesh;
    }

    private Mesh DuplicateMesh(Mesh incommingMesh)
    {
        return new Mesh()
        {
            vertices = incommingMesh.vertices,
            triangles = incommingMesh.triangles,
            normals = incommingMesh.normals,
            tangents = incommingMesh.tangents,
            bounds = incommingMesh.bounds,
            uv = incommingMesh.uv
        };
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "StickyObject")
        {
            if (collision.gameObject.GetComponent<Plant>() != null)
            {
                collision.gameObject.SetActive(false);
                // increase score and stuff
                return;
            }
            uiEvent.Invoke();
            if (collision.gameObject.GetComponent<S_VolumeObject>().Volume* VolumeBias < 
                GetComponent<S_VolumeObject>().Volume)
            {
                GetComponent<S_VolumeObject>().Volume += collision.gameObject.GetComponent<S_VolumeObject>().Volume;
                collision.transform.parent = transform;

                Destroy(collision.gameObject.GetComponent<Rigidbody>());
                collision.gameObject.GetComponent<MeshCollider>().enabled = false;


                InflateAllVerts();
                DistortMesh(collision.gameObject, DistortType.DISTORT_GROUP_TO_ORIGIN);
                
                
                //SphereCollider sphereCollider = GetComponent<SphereCollider>();
                //sphereCollider.center= centroid;
                //sphereCollider.radius= CalculateCombinedBounds().size.magnitude/5;

            }

        }
    }

    const float Volumedampening = 0.01f;
    void InflateAllVerts () 
    {
        Mesh mesh = OriginalCollisionMesh;
            
        Vector3[] newVertices = mesh.vertices;

        int numChildren = transform.childCount;
        float volume = GetComponent<S_VolumeObject>().Volume;

        for(int i =0; i< newVertices.Length; i++)
        {
            
            newVertices[i]+= LerpPoints(transform.InverseTransformPoint(transform.position), newVertices[i], volume*Volumedampening);
        }
        //Assign the new vertices to the old mesh
        mesh.vertices = newVertices;

        //Recalc the normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Assign the new mesh to the old mesh collider
        meshCollider.sharedMesh = mesh;
    }


    const float PercentAgeCloseToObjectMidPoint = 0.4f;
    const float PercentageOfMagnatude = 0.2f;
    /// <summary>
    /// Distorts our collision mesh to include the attached object
    /// </summary>
    /// <param name="attachedObject">The object we've attached to</param>
    /// <param name="distortType">An optional enum dictating the type of distortion to use, defaults to distort the closest point on our mesh to the origin of the object</param>
    protected virtual void DistortMesh(GameObject attachedObject, DistortType distortType = DistortType.DISTORT_TO_ORIGIN)
    {
        //Create a local copy of the mesh first of all
        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] newVertices = mesh.vertices;

        int closestVertexIndex = 0;
        float minDistance = Mathf.Infinity;

        //Scan all vertices of our mesh to find the closest to the attached object
        for (int i = 0; i < newVertices.Length; i++)
        {
            Vector3 diff = transform.InverseTransformPoint(attachedObject.transform.position) - newVertices[i];
            float dist = diff.sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                closestVertexIndex = i;
            }
        }

        //Debug Code to see which vertex is chosen
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = transform.TransformPoint(newVertices[closestVertexIndex]);

        
        switch (distortType)
        {
            case DistortType.DISTORT_TO_ORIGIN:
                {
                    
                    Vector3 midPoint = LerpPoints(transform.position, attachedObject.transform.position, PercentAgeCloseToObjectMidPoint);

                    //Distort our mesh to the origin of the attached object
                    newVertices[closestVertexIndex] = transform.InverseTransformPoint(midPoint);
                    break;
                }
            case DistortType.DISTORT_GROUP_TO_ORIGIN:
            {
                Vector3 midPoint = LerpPoints(transform.position, attachedObject.transform.position, PercentAgeCloseToObjectMidPoint);

                //Distort our mesh to the origin of the attached object
                newVertices[closestVertexIndex] = transform.InverseTransformPoint(midPoint);

                



                for (int i = 0; i < newVertices.Length; i++)
                {
                    if (i != closestVertexIndex)
                    {
                        if (Vector3.Distance(newVertices[i], newVertices[closestVertexIndex]) < 0.15f) // im leaving this magicnumber here as a reminder to find the min distance programaticaly
                        {
                            //newVertices[i] = LerpPoints(newVertices[closestVertexIndex], newVertices[i], 0.75f);
                            
                            newVertices[i]  += Vector3.Normalize(newVertices[i])* newVertices[closestVertexIndex].magnitude*PercentageOfMagnatude;
                        }
                    }
                }

                break;
            }
            case DistortType.DISTORT_TO_FURTHEST_AWAY:
                {
                    //Distort our mesh to the furthest vertices of the attached object

                    //Get the mesh of the attached object
                    Mesh attachedMesh = attachedObject.GetComponent<MeshCollider>().sharedMesh;

                    int furthestVertexIndex = 0;
                    float maxDistance = Mathf.NegativeInfinity;

                    //Scan all vertices to find the one furthest away
                    for (int i = 0; i < attachedMesh.vertexCount; i++)
                    {
                        Vector3 diff = attachedMesh.vertices[i] - attachedObject.transform.InverseTransformPoint(transform.position);
                        float dist = diff.sqrMagnitude;
                        if (dist > maxDistance)
                        {
                            maxDistance = dist;
                            furthestVertexIndex = i;
                        }
                    }

                    newVertices[closestVertexIndex] = transform.InverseTransformPoint(attachedObject.transform.TransformPoint(attachedMesh.vertices[furthestVertexIndex]));
                    break;
                }
            default:
                {
                    throw new NotImplementedException();
                }
        }

        //Assign the new vertices to the old mesh
        mesh.vertices = newVertices;

        //Recalc the normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Assign the new mesh to the old mesh collider
        meshCollider.sharedMesh = mesh;
    }

    Vector3 LerpPoints(Vector3 start, Vector3 end, float percent)
    {
        return (start + percent*(end - start));
    }

    Vector3 CalculateCentroid()
    {
        Vector3 centroid = new Vector3(0, 0, 0);

        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                centroid += child.transform.position;
            }
            centroid /= (transform.childCount + 1);
        }

        return centroid;
    }

    Bounds CalculateCombinedBounds()
    {
        Bounds combinedBounds = this.GetComponent<Renderer>().bounds;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer  render  in renderers)
        {
            if (render != this.GetComponent<Renderer>()) combinedBounds.Encapsulate(render.bounds);
        }

        return combinedBounds;
    }
}

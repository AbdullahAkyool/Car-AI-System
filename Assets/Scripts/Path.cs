using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Color lineColor;
    private List<Transform> Nodes;

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        Transform[] pathTransform = GetComponentsInChildren<Transform>();
        Nodes = new List<Transform>();
        
        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != transform)
            {
                Nodes.Add(pathTransform[i]);
            }
        }

        for (int i = 0; i < Nodes.Count; i++)
        {
            Vector3 currentNode = Nodes[i].position;
            Vector3 previousNode = Vector3.zero;

            if (i > 0)
            {
                previousNode = Nodes[i - 1].position; //bir önceki ile şuanki nokta arasına line çizer
            }
            else if(i==0 && Nodes.Count>1)
            {
                previousNode = Nodes[Nodes.Count - 1].position; //son noktadan ilk noktaya line çizme
            }
            
            Gizmos.DrawLine(previousNode,currentNode);
            Gizmos.DrawWireSphere(currentNode,.8f);
        }
    }
}

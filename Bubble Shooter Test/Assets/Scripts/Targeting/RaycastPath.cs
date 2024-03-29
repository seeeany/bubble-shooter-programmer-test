﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{

    public class RaycastPath : MonoBehaviour
    {
        private const string POOL_IDENTIFIER = "Dots";
        private const string WALL_TAG = "Side Wall";

        public List<Vector3> DotPositions => dotPositions;

        public bool CanAim
        {
            get => canAim;
            set => canAim = value;
        }

        [SerializeField] private Pool pool                  = null;
        [SerializeField] private Transform bubblePosition   = null;
        [SerializeField] private LayerMask RaycastMask      = new LayerMask();

        private List<Vector3> dotPositions;
        private bool canAim = true;

        // Start is called before the first frame update
        private void Start()
        {
            dotPositions = new List<Vector3>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!canAim)
                return;

            if (!pool.IsInitialized(POOL_IDENTIFIER) || dotPositions == null)
                return;
        }

        public void ResetPath()
        {
            if (dotPositions == null || dotPositions.Count < 2)
                return;

            dotPositions.Clear();
            pool.ResetPool(POOL_IDENTIFIER);
        }

        public void GeneratePath(Vector2 touch)
        {
            if (dotPositions == null)
                throw new NullReferenceException("Dot Positions list is null");

            dotPositions.Clear();
            Vector2 point = Camera.main.ScreenToWorldPoint(touch);

            if (!bubblePosition)
                throw new NullReferenceException("The bubble position is null");

            Vector2 direction = new Vector2(point.x - bubblePosition.position.x, point.y - bubblePosition.position.y);
            RaycastHit2D hit = Physics2D.Raycast(bubblePosition.position, direction, RaycastMask);

            Debug.DrawRay(bubblePosition.position, direction.normalized * (Vector2.Distance(new Vector2(bubblePosition.position.x, bubblePosition.position.y), hit.point)));

            if (!hit.collider)
            {
                Debug.Log("Nothing hit");
                return;
            }


            dotPositions.Add(bubblePosition.position);

            if (hit.collider.CompareTag(WALL_TAG))
            {
                CalculatePath(hit, direction);
                return;
            }

            dotPositions.Add(hit.point);
        }

        private void CalculatePath(RaycastHit2D previousHit, Vector2 directionIn)
        {
            while (true)
            {
                if (dotPositions.Count > 1000)
                    return;

                // Add the previous hit to the list of dot positions
                dotPositions.Add(previousHit.point);

                // Calculate the tangent of the previous hit's normal
                float normal = Mathf.Atan2(previousHit.normal.y, previousHit.normal.x);
                // Calculate new direction
                float newDir = normal + (normal - Mathf.Atan2(directionIn.y, directionIn.x));
                // Calculate the reflection vector
                Vector2 reflection = new Vector2(-Mathf.Cos(newDir), -Mathf.Sin(newDir));
                // Create new raycast start point
                Vector2 newCastPoint = previousHit.point + (reflection.normalized / 100);

                RaycastHit2D hit = Physics2D.Raycast(newCastPoint, reflection, RaycastMask);

                Debug.DrawRay(hit.point, hit.normal);
                Debug.DrawRay(previousHit.point, reflection * (Vector2.Distance(previousHit.point, hit.point)));

                if (!hit.collider)
                {
                    break;
                }

                if (hit.collider.CompareTag(WALL_TAG))
                {
                    previousHit = hit;
                    directionIn = reflection;
                    continue;
                }
                else
                {
                    dotPositions.Add(hit.point);
                }

                break;
            }
        }
    }
}
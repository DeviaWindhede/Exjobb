﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulationGenerator : MonoBehaviour
{
    private static List<Triangle> tempTriangles = new List<Triangle>();

    public static List<Triangle> GenerateDelaunayTriangulatedGraph(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = GeneratePoints(pointAmount, bounds);
        tempTriangles = BowyerWatson(points, bounds);
        return tempTriangles;
    }

    public static List<Vector2> GeneratePoints(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++) // possibilty for duplicates
        {
            float x = UnityEngine.Random.Range(0f, bounds.x);
            float y = UnityEngine.Random.Range(0f, bounds.y);
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public static List<Triangle> BowyerWatson(List<Vector2> points, Vector2 bounds)
    {
        List<Triangle> triangulation = new List<Triangle>();

        Triangle superTriangle = GenerateSuperTriangle(bounds);
        triangulation.Add(superTriangle);

        // Add all points sequentially to the triangulation
        foreach (Vector2 point in points)
        {
            // Finds all triangles no longer valid after point insertion
            HashSet<Triangle> badTriangles = new HashSet<Triangle>();
            foreach (Triangle triangle in triangulation)
            {
                if (triangle.IsPointWithinCircumference(point))
                    badTriangles.Add(triangle);
                /*
                Matrix4x4 matrix = GetMatrixFromTriangle(triangle.A, triangle.B, triangle.C, point);
                if (IsPointWithinCircumference(matrix))
                    badTriangles.Add(triangle);
                */
            }

            // Handle bad triangles
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.A, triangle.B));
                edges.Add(new Edge(triangle.B, triangle.C));
                edges.Add(new Edge(triangle.C, triangle.A));
            }   
            
            foreach (Triangle triangle in badTriangles)
                triangulation.Remove(triangle);


            // all edges around triangulation
            HashSet<Edge> boundry = new HashSet<Edge>();
            // Gets edges without duplicates
            boundry = new HashSet<Edge>(edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(x => x.Key));

            // Creates new triangles connecting to added point
            foreach (Edge edge in boundry)
            {
                // forms a triangle from edge to point
                Triangle newTriangle = new Triangle(edge.Point1, edge.Point2, point);
                triangulation.Add(newTriangle);
            }
        }

        // Removes super triangle
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];
            if (triangle.SharesAnyVertex(superTriangle))
                triangulation.RemoveAt(i);
        }

        return triangulation;
    }

    /// <summary>
    /// Generate a right triangle which includes innerbounds fully
    /// </summary>
    /// <param name="innerBounds">Rectangle that must be included</param>
    /// <param name="angle">Aesthetic, leave blank if unsure (0 - 90)</param>
    private static Triangle GenerateSuperTriangle(Vector2 innerBounds, float angle = 45)
    {
        angle = Mathf.Clamp(angle, 0f, 90f);

        float angleA = angle;
        float angleB = 90 - angle;
        Vector2 pointA = new Vector2(0, innerBounds.y + innerBounds.x * Mathf.Tan(angleA * Mathf.Deg2Rad));
        Vector2 pointB = Vector2.zero;
        Vector2 pointC = new Vector2(innerBounds.x + innerBounds.y * Mathf.Tan(angleB * Mathf.Deg2Rad), 0);
        return new Triangle(pointA, pointB, pointC);
    }

    /*
    /// <summary>
    /// Returns matrix used to check triangle circumferences
    /// </summary>
    private static Matrix4x4 GetMatrixFromTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 point)
    {
        Vector4[] rows = new Vector4[4];
        Vector2[] array = new Vector2[]
        {
            A, B, C, point
        };

        for (int i = 0; i < array.Length; i++)
            rows[i] = GetRow(array[i]);

        return new Matrix4x4(rows[0], rows[1], rows[2], rows[3]);
    }

    /// <summary>
    /// Calculate matrix row based on Vector value
    /// </summary>
    private static Vector4 GetRow(Vector2 value)
    {
        return new Vector4(value.x, value.y, SumOfSquaredVectorTerms(value), 1);
    }

    /// <summary>
    /// Returns sum of square of both Vector terms, (Used to calculate M3 in GetRow)
    /// </summary>
    private static float SumOfSquaredVectorTerms(Vector2 value)
    {
        return value.x * value.x + value.y * value.y;
    }

    private static bool IsPointWithinCircumference(Matrix4x4 m)
    {
        return m.determinant > 0;
    }
    */















    //private void OnValidate()
    //{
    //    GetComponent<MeshFilter>().mesh = MeshGenerator.GenerateMesh(values);
    //    if (generate)
    //    {
    //        generate = false;
    //        var points = GeneratePoints(tempPointAmount, tempBounds);
    //        g = BowyerWatson(points);
    //    }
    //}

    private void Start()
    {
        GenerateDelaunayTriangulatedGraph(pointCount, new Vector2(2, 2));
    }

    private static List<Vector2> tempCirclePoints = new List<Vector2>();

    public int pointCount = 100;
    public int index = -1;
    private void OnDrawGizmos()
    {
        /*
        foreach (var point in points)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y, -1), 0.1f);
        }
        */

        UnityEditor.Handles.color = Color.red;
        for (int i = 0; i < tempTriangles.Count; i++)
        {
            Vector2 A = tempTriangles[i].A;
            Vector2 B = tempTriangles[i].B;
            Vector2 C = tempTriangles[i].C;

            float alpha = ((B - C).sqrMagnitude * Vector3.Dot(A - B, A - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
            float beta = ((A - C).sqrMagnitude * Vector3.Dot(B - A, B - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
            float gamma = ((A - B).sqrMagnitude * Vector3.Dot(C - A, C - B)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
            Vector3 center = alpha * A + beta * B + gamma * C;
            float radius = ((A - B).magnitude * (B - C).magnitude * (C - A).magnitude) / (2f * Vector3.Cross(A - B, B - C).magnitude);

            if (index == i || index < 0)
                UnityEditor.Handles.DrawWireDisc(center, Vector3.back, radius);
        }

        Gizmos.color = Color.white;
        foreach (var triangle in tempTriangles)
        {
            Gizmos.DrawLine(triangle.A, triangle.B);
            Gizmos.DrawLine(triangle.B, triangle.C);
            Gizmos.DrawLine(triangle.C, triangle.A);
        }

        Gizmos.color = new Color(0.1f, 0.0f, 1.0f);
        if (true)
        {
            Vector2 tempBounds = new Vector2(2, 2);
            Gizmos.DrawLine(Vector3.zero, Vector2.up * tempBounds.y);
            Gizmos.DrawLine(Vector3.zero, Vector2.right * tempBounds.x);
            Gizmos.DrawLine(Vector2.right * tempBounds.x, Vector2.one * tempBounds);
            Gizmos.DrawLine(Vector2.up * tempBounds.y, Vector2.one * tempBounds);

            Triangle superTriangle = GenerateSuperTriangle(tempBounds);
            Gizmos.DrawLine(superTriangle.A, superTriangle.B);
            Gizmos.DrawLine(superTriangle.B, superTriangle.C);
            Gizmos.DrawLine(superTriangle.C, superTriangle.A);
        }
    }
}

public class MeshGenerator
{
    public static Mesh GenerateMesh(List<Vector2> values)
    {
        int count = values.Count;

        if (count > 0)
        {
            int[] _triangles;
            Vector3[] _vertices;
            Vector3[] _fullVertices;
            Vector2[] _uv;
            Mesh _mesh = new Mesh();

            _vertices = new Vector3[count + 1];
            _uv = new Vector2[count + 1];
            _triangles = new int[3 * count];

            // uv
            _uv[0] = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                _uv[i + 1] = Vector2.one;
            }

            // vertices and tris
            _vertices[0] = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                _vertices[i + 1] = values[i];

                // Tris
                _triangles[i * 3] = 0;
                for (int j = 0; j < 2; j++)
                {
                    _triangles[i * 3 + j + 1] = i + j + 1;
                }
            }
            _triangles[3 * count - 1] = 1;
            _fullVertices = new Vector3[count + 1];
            Array.Copy(_vertices, _fullVertices, _vertices.Length);

            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            return _mesh;
        }
        return null;
    }
}
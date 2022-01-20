using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Boid
{
    public Vector3 position;
    public Vector3 direction;
}

public class GPUDraw : MonoBehaviour
{
    public ComputeShader compute;

    public int BoidsAmount;
    public float SpawnRadius;
    public Boid[] boidsData;
    public Mesh mesh;

    private int kernel;
    private ComputeBuffer buffer;
    public Material boidMat;
    ComputeBuffer drawBuffer;
    MaterialPropertyBlock props;

    public float RotationSpeed = 1f;
    public float BoidSpeed = 1f;
    public float AvoidanceWeight = 1f;
    public float AlignmentWeight = 1f;
    public float CohesionWeight = 1f;

    void Start()
    {
        // Initialize the indirect draw args buffer.
        drawBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
        );

        drawBuffer.SetData(new uint[5] {
            mesh.GetIndexCount(0), (uint) BoidsCount, 0, 0, 0
        });

        props = new MaterialPropertyBlock();
        props.SetFloat("uid", Random.value);

        boidsData = new Boid[BoidsCount];
        kernel = _ComputeFlock.FindKernel("CSMain");

        for (int i = 0; i < BoidsCount; i++)
        {
            Boid boid = new Boid();
            boid.position = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            boid.direction = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            boidsData[i] = boid;
        }

        buffer = new ComputeBuffer(BoidsCount, 24);
        buffer.SetData(boidsData);
    }

    void Update()
    {
        compute.SetFloat("DeltaTime", Time.deltaTime);
        compute.SetFloat("RotationSpeed", RotationSpeed);
        compute.SetFloat("BoidSpeed", BoidSpeed);
        compute.SetInt("BoidsCount", BoidsAmount);
        compute.SetFloat("cohesionWeight", CohesionWeight);
        compute.SetFloat("avoidanceWeight", AvoidanceWeight);
        compute.SetFloat("alignmentWeight", AlignmentWeight);
        compute.SetBuffer(kernel, "boidBuffer", buffer);
        compute.Dispatch(kernel, BoidsAmount / 256 + 1, 1, 1);

        boidMat.SetBuffer("boidBuffer", buffer);
        Graphics.DrawMeshInstancedIndirect(
            mesh, 0, boidMat,
            new Bounds(Vector3.zero, Vector3.one * 2000.0f),
            drawBuffer, 0, props
        );
    }
}
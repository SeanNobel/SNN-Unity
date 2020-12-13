using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public class HHModelCurrentSyn : MonoBehaviour
{
    float C_m = 1f; // 膜容量 (μF/cm^2)
    float g_Na = 120f; // Na+の最大コンダクタンス (mS/cm^2)
    float g_K = 36f;
    float g_L = 0.3f;
    float E_Na = 50f; // Na+の平衡電位 (mV)
    float E_K = -77f;
    float E_L = -54.387f;

    string solver = "Euler";
    float dt = 0.025f;

    public float[] states = {-65f, 0.05f, 0.6f, 0.32f}; // V, m, h, n
    public float I_m = 0f; // 入力電流 (μA/cm^2)
    float input_current = 10f;

    public float isInput = 0;
    float r = 0;
    float td = 0.3f; // synaptic decay time

    Material mat;
    Color colorToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        // mat.EnableKeyword("_EMISSION");
        colorToDisplay = mat.GetColor("_EmissionColor");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // this.dt = Time.deltaTime;
        float[] states = Solvers(this.states, this.dt);
        this.states = states;

        // Debug.Log("Voltage = " + states[0]);
    }

    void Update()
    {
        r = r * (1f - dt / td) + isInput / td;
    }

    void LateUpdate()
    {
        float intensity = this.states[0] * 0.05f;
        float factor = Mathf.Pow(2, intensity);
        mat.SetColor("_EmissionColor", new Color(0.1f*factor, 0.6f*factor, 0.3f*factor));

        if(isInput == 1f) isInput = 0;
    }

    float[] Solvers(float[] x, float dt)
    {
        var x_arr = np.array(x);

        if(this.solver == "RK4")
        {
            var k1_arr = dt * np.array(dALLdt(x_arr));
            var k2_arr = dt * np.array(dALLdt(x_arr + 0.5f * k1_arr));
            var k3_arr = dt * np.array(dALLdt(x_arr + 0.5f * k2_arr));
            var k4_arr = dt * np.array(dALLdt(x_arr + k3_arr));

            var out_arr = x_arr + (k1_arr + 2f * k2_arr + 2f * k3_arr + k4_arr) / 6f;

            return new float[] {out_arr[0], out_arr[1], out_arr[2], out_arr[3]};
        }
        else if(this.solver == "Euler")
        {
            var out_arr = x_arr + dt * np.array(dALLdt(x_arr));

            return new float[] {out_arr[0], out_arr[1], out_arr[2], out_arr[3]};
        }
        else 
        {
            return new float[] {};
        }
    }

    float alpha_m(float V)
    {
        return 0.1f * (40f + V) / (1f - Mathf.Exp(- (40f + V) / 10f));
    }
    float beta_m(float V)
    {
        return 4f * Mathf.Exp(- (V + 65f) / 18f);
    }
    float alpha_h(float V)
    {
        return 0.07f * Mathf.Exp(- (V + 65f) / 20f);
    }
    float beta_h(float V)
    {
        return 1f / (Mathf.Exp(- (35f + V) / 10f) + 1f);
    }
    float alpha_n(float V)
    {
        return 0.01f * (55f + V) / (1f - Mathf.Exp(- (55f + V) / 10f));
    }
    float beta_n(float V)
    {
        return 0.125f * Mathf.Exp(- (V + 65f) / 80f);
    }

    float I_Na(float V, float m, float h)
    {
        return this.g_Na * Mathf.Pow(m, 3f) * h * (V - this.E_Na);
    }
    float I_K(float V, float n)
    {
        return this.g_K * Mathf.Pow(n, 4f) * (V - this.E_K);
    }
    float I_L(float V)
    {
        return this.g_L * (V - this.E_L);
    }

    float[] dALLdt(NDArray states)
    {
        float V = states[0]; float m = states[1]; float h = states[2]; float n = states[3];

        float dVdt = (this.I_m - I_Na(V, m, h) - I_K(V, n) - I_L(V)) / this.C_m;
        float dmdt = alpha_m(V) * (1f - m) - beta_m(V) * m;
        float dhdt = alpha_h(V) * (1f - h) - beta_h(V) * h;
        float dndt = alpha_n(V) * (1f - n) - beta_n(V) * n;

        return new float[] {dVdt, dmdt, dhdt, dndt};
    }
}
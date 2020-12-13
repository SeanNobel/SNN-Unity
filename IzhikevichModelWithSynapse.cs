using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public class IzhikevichModelWithSynapse : MonoBehaviour
{
    double dt = 0.5; // ms

    double C = 100; // 膜容量 (pF)
    double a = 0.03; // 回復時定数の逆数 (1/ms)
    double b = -2; // uのvに対する共鳴度合い (pA/mV)
    double k = 0.7; // ゲイン (pA/mV)
    double d = 100; // 発火で活性化される正味の外向き電流 (pA)
    double vrest = -60; // 静止膜電位 (mV)
    double vreset = -50; // リセット電位
    double vthr = -40; // 閾値電位 (mV)
    double vpeak = 35; // ピーク電位 (mV)

    public double v = -60; double v_; double u;

    public double I = 0; // 入力電流 (pA)
    double injectedCurrent = 0;
    public double currentInjection = 100; // pA

    Material mat;

    string neuronType = "Chattering";

    GameObject spikeManager;
    SpikeManager spikeManagerScript;

    int neuronNum = 0; // 仮の値
    double sumEPSC;
    NDArray connection_weight;

    // SingleExponentialEPSC synapseScript;

    // Start is called before the first frame update
    void Start()
    {
        if(this.neuronType == "IB")
        {
            this.C = 150; this.a = 0.01; this.b = 5; this.k = 1.2; this.d = 130;
            this.vrest = -75; this.vreset = -56; this.vthr = -45; this.vpeak = 50;
            this.currentInjection = 600;
        }
        else if(neuronType == "Chattering")
        {
            this.C = 50; this.a = 0.03; this.b = 1; this.k = 1.5; this.d = 150;
            this.vrest = -60; this.vreset = -40; this.vthr = -40; this.vpeak = 35;
            this.currentInjection = 300;
        }

        this.v = this.vrest;
        this.v_ = this.v;
        this.u = 0;

        this.mat = gameObject.GetComponent<MeshRenderer>().material;


        // 発火管制塔にスパイクを知らせるため
        spikeManager = GameObject.Find("SpikeManager");
        spikeManagerScript = spikeManager.GetComponent<SpikeManager>();
        // 自身に入力するシナプスとその重みを取得
        this.connection_weight = spikeManagerScript.connection_weight[spikeManagerScript.neuronDict[gameObject.name]];
        // ProjectSettingsでSpikeManagerのStart()が先に実行されるようにした

        neuronNum = spikeManagerScript.neuronNum;

        // 自身に入っているシナプス後電流の合計を取得するため
        // this.synapseScript = gameObject.GetComponent<SingleExponentialEPSC>();

        // (16ニューロンのとき)５番目以降のニューロンの入力電流をゼロにする　あとで消せる
        if(spikeManagerScript.neuronDict[gameObject.name] >= 4)
        {
            this.currentInjection = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // synapseScript(SingleExponentialEPSC)からすべての前細胞からのEPSCの合計を取得する
        this.I = SumEPSCInput() + this.injectedCurrent;

        double dv = (this.k * (this.v - this.vrest) * (this.v - this.vthr) - this.u + this.I) / this.C;
        this.v += this.dt * dv;
        this.u += this.dt * (this.a * (this.b * (this.v_ - this.vrest) - this.u));

        int s;
        if(this.v >= this.vpeak) {
            s = 1;
            // スパイクをマネージャーに知らせる
            spikeManagerScript.spike[ spikeManagerScript.neuronDict[gameObject.name] ] = true;
        }
        else {
            s = 0;
        }

        this.u += this.d * s;
        this.v = this.v * (1 - s) + this.vreset * s;
        this.v_ = this.v;
        // Debug.Log(gameObject.name.ToString() + this.v.ToString());
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            this.injectedCurrent = this.currentInjection;
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            this.injectedCurrent = 0;
        }
    }

    void LateUpdate()
    {
        float intensity = (float)v * 0.05f;
        float factor = Mathf.Pow(2, intensity);
        this.mat.SetColor("_EmissionColor", new Color(0.1f*factor, 0.6f*factor, 0.3f*factor));
    }

    double SumEPSCInput()
    {
        double sum = 0;
        for(int i=0; i<neuronNum; i++)
        {
            sum += spikeManagerScript.r[i] * this.connection_weight[i];
        }

        return sum;
    }
}

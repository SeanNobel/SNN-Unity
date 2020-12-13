using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

// EPSCも管理するスクリプト
// ニューロンの生成もする
public class SpikeManager : MonoBehaviour
{
    public int neuronNum = 16; // 平方数じゃないとGenerateNeurons()でバグる
    GameObject Neurons;
    // ニューロンオブジェクトの名前と番号(0~)を紐づける辞書
    public Dictionary<string, int> neuronDict = new Dictionary<string, int>();

    public GameObject izhikevichPrefab;

    public bool[] spike;// falseで初期化される

    // シナプスコネクションと重みのテーブル(ニューロン数 x ニューロン数)　横軸->縦軸の接続　0 or 1
    public NDArray connection_weight;
    double[] rawEPSC;

    double dt = 0.5; // ms
    double td = 100; // synaptic decay time (ms)
    double tr = 2; // synaptic rise time (ms)

    public double[] r; // 後細胞に与えるEPSCのシナプス重みを掛ける前のものを全ニューロンについて格納 (raw EPSC)
    double[] hr;


    // Start is called before the first frame update 最初に実行されるStart()
    void Start()
    {
        Neurons = GameObject.Find("Neurons");
        GenerateNeurons();

        int n = 0;
        foreach(Transform child in Neurons.transform)
        {
            neuronDict.Add(child.name, n);
            n += 1;
        }
        Debug.Log(neuronDict);

        if(n != neuronNum)　Debug.Log("ニューロン数とオブジェクト数が合いません。");

        spike = new bool[neuronNum]; rawEPSC = new double[neuronNum];
        r = new double[neuronNum]; hr = new double[neuronNum]; // ゼロで初期化

        // ニューロン数xニューロン数行列、0~500のランダムな整数（シナプス重み）
        connection_weight = np.random.randint(500, 1500, (neuronNum, neuronNum));
        // 自分自身への投射と逆方向への投射をすべてゼロにする
        for(int i=0; i<neuronNum; i++) {
            for(int j=0; j<neuronNum; j++) {
                if(i<=j) connection_weight[i,j] = 0;
            }
        }
    }

    void FixedUpdate()
    {
        // 全ニューロンに関して同じようにraw EPSCを計算する
        for(int i=0; i<neuronNum; i++)
        {
            r[i] = r[i] * (1 - dt / tr) + hr[i] * dt;
            hr[i] = hr[i] * (1 - dt / td) + Convert.ToInt32(spike[i]) / (tr * td);

            if(spike[i]) spike[i] = false;
        }
    }

    void LateUpdate()
    {

    }

    void GenerateNeurons()
    {
        int n = 0;
        for(int i=0; i<Convert.ToInt32(Mathf.Sqrt((float)neuronNum)); i++)
        {
            for(int j=0; j<Convert.ToInt32(Mathf.Sqrt((float)neuronNum)); j++)
            {
                n += 1;

                GameObject obj = Instantiate(izhikevichPrefab, new Vector3((float)i*2f, 0.1f, (float)j*2f), Quaternion.identity) as GameObject;
                obj.transform.parent = Neurons.transform;
                obj.name = "IzhikevichPrefab" + n.ToString();
            }
        }
    }
}

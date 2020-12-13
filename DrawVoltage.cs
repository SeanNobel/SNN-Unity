using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NumSharp;

public class DrawVoltage : MonoBehaviour
{
    LineRenderer renderer;
    Canvas canvas;

    int plotNum = 500;
    float[] data;
    float dataToAdd;

    string modelType = "Izhikevich";
    HodgkinHuxleyModel HHscript;
    IzhikevichModelWithSynapse izhikevichscript;

    Text voltage_text;
    Text inputCurrent_text;
    float inputCurrent;

    // Start is called before the first frame update
    void Start()
    {
        data = new float[plotNum];

        if(modelType == "HH")
        {
            HHscript = GameObject.Find("HH-Neuron").GetComponent<HodgkinHuxleyModel>();
            for(int i=0; i<plotNum; i++) data[i] = HHscript.states[0];
        }
        else if(modelType == "Izhikevich")
        {
            izhikevichscript = GameObject.Find("IzhikevichPrefab1").GetComponent<IzhikevichModelWithSynapse>();
            for(int i=0; i<plotNum; i++) data[i] = (float)izhikevichscript.v;
        }

        voltage_text = GameObject.Find("VoltageText").GetComponent<Text>();
        inputCurrent_text = GameObject.Find("InputCurrentText").GetComponent<Text>();


        renderer = gameObject.GetComponent<LineRenderer>();
        canvas = gameObject.GetComponent<Canvas>();

        renderer.SetWidth(0.05f, 0.05f); // 線の太さ
        renderer.SetVertexCount(plotNum); // 頂点の数
        

        for(int i=0; i<plotNum; i++)
        {
            renderer.SetPosition(i, convertWorldToCanvas(new Vector2((i-plotNum/2)*10f/plotNum, FitDataToGraph(data[i])), canvas));
            //Debug.Log(data[i]);
        }
    }

    private Vector3 convertWorldToCanvas(Vector2 pos, Canvas canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        pos.x += canvasRect.transform.position.x;
        pos.y += canvasRect.transform.position.y;
        return new Vector3(pos.x, pos.y, canvasRect.transform.position.z - 0.01f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateData();

        for(int i=0; i<plotNum; i++)
        {
            renderer.SetPosition(i, convertWorldToCanvas(new Vector2((i-plotNum/2)*10f/plotNum, FitDataToGraph(data[i])), canvas));
        }
    }

    // データ配列を更新
    void UpdateData()
    {
        List<float> dataList = data.ToList();

        if(modelType == "HH") {
            dataToAdd = HHscript.states[0];
            inputCurrent = HHscript.I_m;
        }
        else if(modelType == "Izhikevich") {
            dataToAdd = (float)izhikevichscript.v;
            inputCurrent = (float)izhikevichscript.I;
        }

        dataList.Insert(0, dataToAdd);
        dataList.RemoveAt(plotNum);
        data = dataList.ToArray();

        voltage_text.text = dataToAdd.ToString("F2") + " mV";
        inputCurrent_text.text = "Input current: " + inputCurrent.ToString("F0") + " pA";
    }

    // データ(-80~70mVくらい)をグラフの大きさ(ワールド座標-1.2~3.8くらい)にコンバート
    float FitDataToGraph(float v)
    {
        return v / 30f;
    }
}

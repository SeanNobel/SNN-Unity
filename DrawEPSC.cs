using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NumSharp;

public class DrawEPSC : MonoBehaviour
{
    LineRenderer renderer;
    Canvas canvas;

    int plotNum = 500;
    float[] data;
    float dataToAdd;

    SingleExponentialEPSC EPSCscript;

    Text EPSC_text;

    // Start is called before the first frame update
    void Start()
    {
        data = new float[plotNum];


        EPSCscript = GameObject.Find("PostNeuron").GetComponent<SingleExponentialEPSC>();
        for(int i=0; i<plotNum; i++) data[i] = (float)EPSCscript.EPSC;

        EPSC_text = GameObject.Find("EPSCText").GetComponent<Text>();


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

    // RectTransformはCanvasの角度情報を持たない？　キャンバスを傾けても追従しないので注意
    private Vector3 convertWorldToCanvas(Vector2 pos, Canvas canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        pos.x += canvasRect.transform.position.x;
        pos.y += canvasRect.transform.position.y;
        // return new Vector3(pos.x, pos.y, canvasRect.transform.position.z - 0.01f);
        return new Vector3(canvasRect.transform.position.z + 6.3f, pos.y, -pos.x + 6.3f);
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

        dataToAdd = (float)EPSCscript.EPSC;

        dataList.Insert(0, dataToAdd);
        dataList.RemoveAt(plotNum);
        data = dataList.ToArray();

        EPSC_text.text = dataToAdd.ToString("F3") + " pA";
    }

    // データ(-80~70mVくらい)をグラフの大きさ(ワールド座標-1.2~3.8くらい)にコンバート
    float FitDataToGraph(float v)
    {
        return v - 2f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawPostNeuronVoltage : MonoBehaviour
{
    [SerializeField] GameObject voltageTextObject;
    Text voltage_text;

    [SerializeField] GameObject postNeuron;
    IzhikevichModelWithSynapse postNeuronScript;

    // Start is called before the first frame update
    void Start()
    {
        voltage_text = voltageTextObject.GetComponent<Text>();

        postNeuronScript = postNeuron.GetComponent<IzhikevichModelWithSynapse>();
    }

    // Update is called once per frame
    void Update()
    {
        voltage_text.text = postNeuronScript.v.ToString();
    }
}

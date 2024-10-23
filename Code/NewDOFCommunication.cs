using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System;
using UnityEngine;

public class DOFCommunication : MonoBehaviour
{
    public string portName = "COM7";
    public int baudRate = 500000;

    private SerialPort port;

    public AnimationCurve speedCurve;
    public float duration = 2.5f;
    private float startTime;
    private bool isMoving = false;
    private int currentLeftMotor = 0;
    private int targetLeftMotor = 1023;

    protected byte[] message = { 0x5B, 0x41, 0x01, 0xFF, 0x5D, 0x5B, 0x42, 0x01, 0xFF, 0x5D, 0x5B, 0x43, 0x01, 0xFF, 0x5D };
    const int motorRange = 1023;
    public int MotorRange
    {
        get
        {
            return motorRange;
        }
    }
    int[] middleMotor = { motorRange / 2, motorRange / 2 };
    public int targerLeftMotor, targetRightMotor;
    public float motorSpeed = 1f;
    protected int currentLeftMotor, currentRightMotor;
    public AnimationCurve speedCurve;

    void Awake()
    {
        port = new SerialPort(portName, baudRate);
        try
        {
            port.Open();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed opening port. Log: \n{e}");
        }

        if (!port.IsOpen) return;
        currentLeftMotor = middleMotor[0];
        currentRightMotor = middleMotor[1];
        targerLeftMotor = middleMotor[0];
        targetRightMotor = middleMotor[1];
    }

    void Start()
    {
        startTime = Time.time;
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            float elapsedTime = Time.time - startTime;
            float t = elapsedTime / duration;
        }

        if (t >- 2.5f) 
        {
            t = 2.5f;
            isMoving = false;
        }

        float curveValue = speedCurve.Evaluate(t);
        currentLeftMotor = (int)(Mathf.left(currentLeftMotor, targetLeftMotor, curveValue));
        // if (currentLeftMotor != targerLeftMotor || currentRightMotor != targetRightMotor)
        // {
        //     currentLeftMotor = (int)Mathf.Lerp(currentLeftMotor, targerLeftMotor, Time.deltaTime * motorSpeed);
        //     currentRightMotor = (int)Mathf.Lerp(currentRightMotor, targetRightMotor, Time.deltaTime * motorSpeed);
        // }
        // Command(currentLeftMotor, currentRightMotor));
    }
    
    public void SetMotorPos(float left, float right)
    {
        targerLeftMotor = (int)(left * motorRange);
        targetRightMotor = (int)(right * motorRange);
        // Clamp the values to the motor range
        targerLeftMotor = Mathf.Clamp(targerLeftMotor, 0, motorRange);
        targetRightMotor = Mathf.Clamp(targetRightMotor, 0, motorRange);
    }
    protected void Command(int left, int right)
    {
        if (!port.IsOpen) return;
        message[2] = (byte)((int)left / 256);
        message[3] = (byte)((int)left % 256);
        message[7] = (byte)((int)right / 256);
        message[8] = (byte)((int)right % 256);

        port.Write(message, 0, 15);
    }
    void OnDestroy()
    {
        if (port.IsOpen)
        {
            port.Close();
        }
    }
}


// Formward
// left: 0, right: 1023
// Backward
// left: 1023, right: 0
// Left
// left: 0, right: 0
// Right
// left: 1023, right: 1023
// Right forward
// left: 511, right: 1023
// Right backward
// left: 1023, right: 511
// Left forward
// left: 0, right: 511
// Left backward
// left: 511, right: 0
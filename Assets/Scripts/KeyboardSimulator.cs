using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeyboardSimulator : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int KEYEVENTF_KEYDOWN = 0x0000;
    private const int KEYEVENTF_KEYUP = 0x0002;

    // Virtual key codes
    private const byte VK_R = 0x52;
    private const byte VK_G = 0x47;
    private const byte VK_E = 0x45;
    private const byte VK_T = 0x54;
    private const byte VK_Q = 0x51;

    private void PressKey(byte keyCode)
    {
        Debug.Log($"Simulating key press: {(char)keyCode}");

        keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public void PressRKey()
    {
        PressKey(VK_R);
    }

    public void PressGKey()
    {
        PressKey(VK_G);
    }

    public void PressEKey()
    {
        PressKey(VK_E);
    }

    public void PressTKey()
    {
        PressKey(VK_T);
    }

    public void PressQKey()
    {
        PressKey(VK_Q);
    }
}

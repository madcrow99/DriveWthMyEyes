# DriveWthMyEyes 
***work in progress***
This project is for educational purposes and not intended to be used in real world scenarios. Do not use any part of this repository to drive/operate a wheelchair. No part of this repository guaranteed or warrented to work even as intended.

This project uses Microsoft's Windows Eye Control to use eye gaze to control external devices. The UWP Windows app uses the Windows eye control API and communicates with an Adafruit ESP32 Feather with Serial UART. The Feather then interfaces a power wheelchair, phone and computer. The computer has separate profiles for basic mouse control, Roblox and Minecraft gaming profiles. The phone and computer controls use HID bluetooth low energy running on Adafruit MCUs using the amazing Adafruit ble libraries. More detail is still to come. Please see https://github.com/madcrow99/BLE-Gyro-HID in the interim.

The following guide describes process of setting up a pc with Windows Eye Control using a compatible eye tracking device.
https://support.microsoft.com/en-us/windows/get-started-with-eye-control-in-windows-1a170a20-1083-2452-8f42-17a7d4fe89a9

The following guide describes how to set up visual studio with the required tools for UWPapps.
https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b

The following is a great resource for gaze interaction concepts and how to use Microsoft's Gaze Interaction Library. The requirements near the end of the page need to be included in the project in order to use gaze interaction.
https://learn.microsoft.com/en-us/windows/communitytoolkit/gaze/gazeinteractionlibrary

# DriveWthMyEyes 
***work in progress***
This project uses Microsoft's Windows Eye Control to use eye gaze to control external devices. The UWP Windows app uses the Windows eye control API and communicates with an Adafruit ESP32 Feather with Serial UART. The Feather then interfaces a power wheelchair, phone and computer. The computer has separate profiles for basic mouse control, Roblox and Minecraft gaming profiles. The phone and computer controls use HID bluetooth low energy running on Adafruit MCU using the amazing Adafruit ble libraries.

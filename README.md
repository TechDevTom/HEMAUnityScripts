# HEMA Simulator Project
---

By: Eric Reesor, Conlan LaFreniere
Supervised by: Dr. Samuel Ajila

## Repository Format
---

Unity projects get very large, so to prevent redundant uploading of flat files (e.g .obj files)  this repository will only contain small, highly altered files (e.g scripts, documentation). Respository contents should be placed in the "Assets" folder of the Unity project.  

### Contents
---
README.md - This file

__Documentation__ - Folder containing all official HEMA Simulator project documentation
* HEMA Simulator - Project Proposal.docx
* HEMA System Design Documentation.docx
* __Diagrams__ - Folder containing all .xml diagram files
  * High Level Class Diagram.xml
  * Input Dataflow Diagram.xml
  * System Structure Diagram.xml
  * Use Case Description.xml

__Scripts__ - Folder containing all .cs script files
* SystemController.cs
* PlayerController.cs
* OpponentController.cs
* GhostSwordController.cs
* MovementCreatorController.cs
* Math3d.cs

__Scenes__ - Folder containing all .unity scene files
* StartMenu.unity
* TrainingScenario.unity
* CreateMovement.unity

__Resources__ - Folder containing exported movement .txt files
* test.txt

__Arduino__ - Folder containing all .ino files
* Bluetooth_and_imu.ino
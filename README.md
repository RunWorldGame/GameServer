1. Install Cinemachine
2. copy the whole unity folder so you can open two projects with unity editor
3. delete the assets folder in the second project
4. create a simlink between the assets folder of the first project assets folder and a new assets folder in the second project
5. Windows: mklink /J "E:\Coding\Schule\JavaGame\ServerClient\Unity\My project (4) - Kopie\Assets" "E:\Coding\Schule\JavaGame\ServerClient\Unity\My project (4)\Assets"
6. open both projects with unity editor
7. every script you change in the first project now appears in the second project
8. In GameNetworkInitializer change the value in one instance to true and one to false
9. Enjoy the 3 cubes moving in sync

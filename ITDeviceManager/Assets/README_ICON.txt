HUONG DAN THEM ICON UNG DUNG

1. Chuan bi file ICO, nen chua cac kich thuoc 16x16, 32x32, 48x48 va 256x256.
2. Dat file voi dung ten:
   ITDeviceManager\Assets\App.ico
3. Khong can sua csproj. Project V1.2.0 da co cau hinh:
   <ApplicationIcon Condition="Exists('Assets\App.ico')">Assets\App.ico</ApplicationIcon>
4. Chay clean/build lai:
   clean.bat
   build.bat
5. AppForm se tu lay icon cua file EXE, nen icon se hien tren title bar cua cac form.
6. Khi publish, file EXE cung mang icon nay.

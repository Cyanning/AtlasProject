
骨性标志
step-1:在Assets/StreamingAssets/Test/下找到Android模型zip,把当前目录文件【替换】掉

step0:在Assets/Plugins/c#/TestGetData.cs 中找到 TEST_NEW_RESOURCE_PATH 和 TEST_NEW_MODEL_MALE_PATH
变量，更改为你的项目地址，如：变量TEST_RESOURCE_PATH

step1:在ClickEvent.cs 的Start()方法中添加需要调试的所有数据TestDatas.
：可以通过代码把数据库读出来，生成TestDatas.Add("1000032");TestDatas.Add("1000032");
的字符串再拷贝进来

step2:运行unity ,
    点击贴图（附加材质，多次点击切换面、标志）；
    点镜头（上半屏居中or全屏）：切换到全屏居中，然后选择模型的标志，调整观看视角。
    点保存：保存相机位置和视角，再点镜头，切换到半屏居中
    点切换视角：测试模型转换效果

step3:确认无误后，在Assets/Txt 目录下查找数据文件。名字为：模型value_颜色.txt


注：如果完成一组骨性标志后，导入到测试后台，进行全流程测试，无误后，重复以上
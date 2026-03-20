using System;
using UnityEditor;
using UnityEngine;

namespace Plugins.C_.ImageCaptureTool
{
    public class ImageCaptureToolEditor : EditorWindow
    {
        #region Filed

        public string outputDirectory;

        public string outputFileName = ImageCaptureTool.DefaultOutputFileName;

        public Camera camera;

        public int imageWidth;

        public int imageHeight;

        public int imageScale = 1;

        public bool clearBack;

        private Vector2 _scrollPosition = Vector2.zero;

        #endregion Field

        #region Method

        [MenuItem("Tools/ImageCapture")]
        private static void Init()
        {
            GetWindow<ImageCaptureToolEditor>("ImageCaptureTool");
        }

        protected void OnEnable()
        {
            EditorApplication.update += ForceOnGUI;
        }

        protected void OnDisable()
        {
            EditorApplication.update -= ForceOnGUI;
        }

        protected void OnGUI()
        {
            var marginStyle = GUI.skin.label;
            marginStyle.wordWrap = true;
            marginStyle.margin = new RectOffset(5, 5, 5, 5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
            var gameViewResolution = GetGameViewResolution();

            if (GUILayout.Button("Click to Capture"))
            {
                HookAfterImageCaptured(Capture());
            }

            // Output directory.

            EditorGUILayout.LabelField("Output Directory", marginStyle);

            EditorGUILayout.BeginHorizontal(GUI.skin.label);
            {
                if (GUILayout.Button("Open"))
                {
                    var tempPath = EditorUtility.SaveFolderPanel("Open", outputDirectory, "");

                    if (!tempPath.Equals(""))
                    {
                        outputDirectory = EditorGUILayout.TextField(tempPath);
                        Repaint();
                    }
                }
                else
                {
                    outputDirectory = EditorGUILayout.TextField(outputDirectory);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                // NOTE:
                // Application.dataPath + "/"; is not so bad.
            }

            // Base setttings.

            EditorGUILayout.LabelField("Base File Name.", marginStyle);
            outputFileName = EditorGUILayout.TextField(outputFileName);

            EditorGUILayout.LabelField("Target camera. When 'null', use 'MainCamera' automatically.", marginStyle);
            camera = EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true) as Camera;

            EditorGUILayout.LabelField("Image width(px). When '0', use GameView width '" + gameViewResolution[0] + "'",
                marginStyle);
            imageWidth = EditorGUILayout.IntSlider(imageWidth, 0, 9999);

            EditorGUILayout.LabelField(
                "Image height(px). When '0', use GameView height '" + gameViewResolution[1] + "'", marginStyle);
            imageHeight = EditorGUILayout.IntSlider(imageHeight, 0, 9999);

            EditorGUILayout.LabelField("Image scale. Ex: When set '2', the result will twice size of width and height.",
                marginStyle);
            imageScale = EditorGUILayout.IntSlider(imageScale, 1, 10);

            EditorGUILayout.LabelField("Clear the background when capture.", marginStyle);
            clearBack = EditorGUILayout.Toggle(clearBack);

            EditorGUILayout.EndScrollView();
        }

        protected void ForceOnGUI()
        {
            // NOTE:
            // Need periodic repaint to update Game View.Resolution info.

            if (DateTime.Now.Millisecond % 5 == 0)
            {
                Repaint();
            }
        }

        protected int[] GetGameViewResolution()
        {
            // NOTE:
            // Screen.width (& height) shows active window's resorution.
            // So in sometimes, it shows EditorWindow's resolution.

            var gameViewResolution = UnityStats.screenRes.Split('x');

            return new [] { int.Parse(gameViewResolution[0]), int.Parse(gameViewResolution[1]) };
        }

        protected ImageCaptureTool.CaptureResult Capture()
        {
            var cameraNow = camera ?? SceneView.lastActiveSceneView.camera;

            var gameViewResolution = GetGameViewResolution();
            var imageWidthNow = (imageWidth == 0 ? gameViewResolution[0] : imageWidth) * imageScale;
            var imageHeightNow = (imageHeight == 0 ? gameViewResolution[1] : imageHeight) * imageScale;

            var result
                = ImageCaptureTool.Capture(cameraNow,
                    imageWidthNow,
                    imageHeightNow,
                    clearBack,
                    outputDirectory,
                    outputFileName
                );

            var infomation = result.success ? "SUCCESS : " : "ERROR : ";
            ShowNotification(new GUIContent(infomation + result.outputPath));
            return result;
        }

        protected virtual void HookAfterImageCaptured(ImageCaptureTool.CaptureResult result)
        {
            // Nothing to do in here. This is used for inheritance.
        }

        #endregion Method
    }
}

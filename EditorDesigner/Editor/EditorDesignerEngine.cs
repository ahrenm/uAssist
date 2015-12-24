namespace uAssist.EditorDesigner
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UEditorWidgets;
    using uAssist.EditorDesigner;
    using uAssist.Forms;
    using System.Collections;
    using System.Collections.Generic;
    
    [InitializeOnLoad]
    public static class EditorDesignerEngine
    {
        [InitializeOnLoadMethod]
        public static void DesignerInit()
        {

            //If the editor is loading for the first time with no level loaded.
            if (Application.levelCount == 0)
            {
                EditorApplication.update += DesignerStateCleaner;
            }
        }

        //Removes any open designer windows at Editor Load time
        private static void DesignerStateCleaner()
        {
            EditorApplication.update -= DesignerStateCleaner;

            var __foundCanvas = Resources.FindObjectsOfTypeAll<frmCanvas>();
            foreach (var item in __foundCanvas)
            {
                item.CloseWindow = true;
                //ScriptableObject.DestroyImmediate(item);
            }

            var __foundToolBox = Resources.FindObjectsOfTypeAll<frmToolbox>();
            foreach (var item in __foundToolBox)
            {
                item.CloseWindow = true;
                //ScriptableObject.DestroyImmediate(item);
            }

            var __foundPropertys = Resources.FindObjectsOfTypeAll<frmPropertyWindow>();
            foreach (var item in __foundPropertys)
            {
                item.CloseWindow = true;
                //ScriptableObject.DestroyImmediate(item);
            }

        }


        [MenuItem("Window/uAssist/Window Designer/Create New Form")]
        public static void CreateNewForm()
        {
            EditorDesignerEngine.LoadEditor<frmBase>();
        }


        [MenuItem("Window/uAssist/Window Designer/Edit Form")]
        public static void EditFrom()
        {
            UnityEditor.EditorWindow.GetWindow<frmOpenDesigner>(true, "Open Form");
        }


        public static void LoadEditor<T>() where T : frmBase
        {
            EditorDesignerEngine.LoadEditor(typeof(T));
        }

        private static frmCanvas _windowCanvas;
        public static frmCanvas Window_Canvas
        {
            get
            {
                if (EditorDesignerEngine._windowCanvas == null)
                {
                    frmCanvas[] __foundCanvas = Resources.FindObjectsOfTypeAll<frmCanvas>();
                    if (__foundCanvas.Count() > 1)
                    {
                        //Try to determine the active window by seeing which instance is listenting for events.
                        //This should be a fringe case when the Designer window is loaded into the designer.
                        for (int i = 0; i < __foundCanvas.Count(); i++)
                        {
                            if (__foundCanvas[i].EventsEnabled == true)
                            {
                                EditorDesignerEngine._windowCanvas = __foundCanvas[i];
                            }
                        }
                    }
                    if (__foundCanvas.Count() == 1)
                    {
                        EditorDesignerEngine._windowCanvas = __foundCanvas[0];
                    }
                    else
                    {
                        EditorDesignerEngine._windowCanvas = UnityEditor.EditorWindow.GetWindow<frmCanvas>(typeof(SceneView));
                    }
                }
                return EditorDesignerEngine._windowCanvas;
            }
            set
            {
                EditorDesignerEngine._windowCanvas = value;
            }

        }

        public static void LoadEditor(Type frmBaseSubClass)
        {
            //UWidget.SeralizationEnabled = false;
            EditorDesignerEngine.Window_Canvas.Show();
            EditorDesignerEngine.Window_Canvas.LoadForm(frmBaseSubClass);
            EditorDesignerEngine.Window_Canvas.PropertyPanel.Canvas_CanvasActiveControlChanged(EditorDesignerEngine.Window_Canvas.ToolBox.__widgetDesignerRoot);
            
        }
    }
}


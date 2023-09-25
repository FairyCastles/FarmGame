using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    int sceneIndex = -1;
    GUIContent[] sceneNames;

    // 路径分隔符
    readonly string[] scenePathSplit = { "/", ".unity" };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;

        if (sceneIndex == -1)
            GetSceneNameArray(property);

        int oldIndex = sceneIndex;

        // 创建下拉菜单，并获取选项值的序号
        sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);

        // 选择了新的选项，更新值
        if (oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }

    private void GetSceneNameArray(SerializedProperty property)
    {
        // 获取 BuildSettings 中所有场景
        var scenes = EditorBuildSettings.scenes;
        // 初始化数组
        sceneNames = new GUIContent[scenes.Length];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string path = scenes[i].path;
            string[] splitPath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);

            string sceneName;

            if (splitPath.Length > 0)
            {
                sceneName = splitPath[splitPath.Length - 1];
            }
            else
            {
                sceneName = "(Deleted Scene)";
            }
            sceneNames[i] = new GUIContent(sceneName);
        }

        // 在 BuildSettings 中没有场景
        if (sceneNames.Length == 0)
        {
            sceneNames = new[] { new GUIContent("Check Your Build Settings") };
        }

        // 该特性变量已经有值
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool nameFound = false;

            // 找到当前值与对应的特性
            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }
            if (nameFound == false)
                sceneIndex = 0;
        }
        else
        {
            sceneIndex = 0;
        }

        property.stringValue = sceneNames[sceneIndex].text;
    }
}

#endif
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomStateMachine;

namespace StateInspectorWindow
{
    public class StateInspector : EditorWindow
    {
        string searchField;
        Vector2 scrollSearchResult, scrollActions, scrollConnections;
        [System.Flags]
        enum SearchFilter
        {
            //Actions                             = 1 << 1,
            //Connections                         = 1 << 2,
            MissingAction                       = 1 << 3,
            MissingConnection                   = 1 << 4,
            IncompleteConnection                = 1 << 5
        }
        SearchFilter searchFilter;
        Dictionary<State, SearchFilter> searchResults;
        bool showResults;
        State selected;
        bool repaint;

        [MenuItem("Window/State Inspector")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(StateInspector));
        }

        static void Init()
        {
            StateInspector stateInspector = (StateInspector)EditorWindow.GetWindow(typeof(StateInspector));
            stateInspector.Show();
        }

        private void OnGUI()
        {
            int margin = 5;
            int searchColumnWidth = 300;
            int columnHeight = 500;

            #region Search Column
            GUILayout.BeginArea(new Rect(margin, margin, searchColumnWidth, columnHeight));

            #region Search Filter Enum Flags
            GUILayout.BeginHorizontal();
            searchFilter = (SearchFilter)EditorGUILayout.EnumFlagsField("Show States with: ", searchFilter);
            GUILayout.EndHorizontal();
            #endregion

            #region Search Bar and Refresh Button + Text Field
            GUILayout.BeginHorizontal();
            // clear the search bar
            if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(13)))
                searchField = "";

            // search bar
            searchField = GUILayout.TextField(searchField, GUILayout.Width(200));

            // search button
            if (GUILayout.Button("Refresh"))
            {
                searchResults = Search();
                showResults = true;
            }
            GUILayout.EndHorizontal();
            #endregion

            #region EnumFlag Icon Labels
            GUILayout.BeginHorizontal();
            GUILayout.Space(200); // width of a result button below

            GUIStyle filterCategories = new GUIStyle();
            filterCategories.normal.textColor = Color.white;
            filterCategories.fixedWidth = 25;
            filterCategories.margin.left = 5;
            filterCategories.padding.left = 3;
            filterCategories.fontSize = 16;
            GUILayout.Label(new GUIContent("\u21BB", "null action slot"), filterCategories);
            GUILayout.Label(new GUIContent("\u2325", "null connection slot"), filterCategories);
            filterCategories.padding.left = 5;
            filterCategories.fontSize = 21;
            GUILayout.Label(new GUIContent("\u2292", "connection does not lead to state"), filterCategories);

            GUILayout.EndHorizontal();
            #endregion

            if (!showResults) // end early here
            {
                GUILayout.EndArea();
                return;
            }

            #region Search Results
            GUILayout.BeginScrollView(scrollSearchResult);

            int alternatingBackgroundColourCounter = 0;
            foreach (var keyValuePair in searchResults)
            {
                GUILayout.BeginHorizontal();

                if (!((keyValuePair.Value & searchFilter) != 0))
                    continue; // flags dont match? skip iteration

                var searchResultContent = EditorGUIUtility.ObjectContent(keyValuePair.Key, typeof(State));

                GUIStyle searchResultButton = new GUIStyle();

                int buttonHeight = 18;
                int buttonWidth = 200;
                Color32 btn_one = new Color32(45, 45, 45, 255);
                Color32 btn_two = new Color32(75, 75, 75, 255);
                Color32 btn_hov = new Color32(255, 176, 0, 255);
                Color32 btn_sel = new Color32(224, 103, 27, 255);
                searchResultButton.normal.textColor = Color.white;
                searchResultButton.normal.background = MakeTexture(buttonWidth, buttonHeight, alternatingBackgroundColourCounter == 0 ? btn_one : btn_two);
                searchResultButton.hover.background = MakeTexture(buttonWidth, buttonHeight, btn_hov);
                searchResultButton.active.background = MakeTexture(buttonWidth, buttonHeight, btn_sel);
                alternatingBackgroundColourCounter++;
                if (alternatingBackgroundColourCounter > 1)
                    alternatingBackgroundColourCounter = 0;

                // keep button fake selected colour after being pressed
                if (keyValuePair.Key == selected)
                    searchResultButton.normal.background = MakeTexture(buttonWidth, buttonHeight, btn_sel);

                if (GUILayout.Button(searchResultContent, searchResultButton, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)) && keyValuePair.Key)
                {
                    EditorGUIUtility.PingObject(keyValuePair.Key);
                    selected = keyValuePair.Key;
                }

                GUIStyle filterFlagIcons = new GUIStyle();
                Color32 good = new Color32(26, 133, 255, 255);
                Color32 bad = new Color32(212, 17, 89, 255);
                filterFlagIcons.fixedWidth = 25;
                filterFlagIcons.margin.left = 5;

                filterFlagIcons.normal.textColor = keyValuePair.Value.HasFlag(SearchFilter.MissingAction) ? bad : good;
                GUILayout.Label("\u25CF", filterFlagIcons);

                filterFlagIcons.normal.textColor = keyValuePair.Value.HasFlag(SearchFilter.MissingConnection) ? bad : good;
                GUILayout.Label("\u25B2", filterFlagIcons);

                filterFlagIcons.normal.textColor = keyValuePair.Value.HasFlag(SearchFilter.IncompleteConnection) ? bad : good;
                GUILayout.Label("\u25C6", filterFlagIcons);

                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            GUILayout.EndScrollView();
            #endregion Search Results
            GUILayout.EndArea();
            #endregion Search Column

            #region Divider
            int dividerWidth = 2;
            GUILayout.BeginArea(new Rect(margin + searchColumnWidth, margin, dividerWidth, columnHeight));
            GUI.color = Color.black;
            GUI.Box(new Rect(0, 0, dividerWidth, columnHeight), "");
            GUILayout.EndArea();
            #endregion

            if (selected == null) // end early again
            {
                return;
            }

            #region Selected State Column
            int selectedStateWidth = 600;
            GUILayout.BeginArea(new Rect(margin + searchColumnWidth + dividerWidth, margin, selectedStateWidth, columnHeight));
            GUI.color = Color.white;
            GUI.Box(new Rect(0, 0, selectedStateWidth, columnHeight), "");

            GUILayout.BeginVertical();
            var selectedStateContent = EditorGUIUtility.ObjectContent(selected, typeof(State));
            EditorGUILayout.LabelField(selectedStateContent);

            GUI.color = Color.black;
            GUI.Box(new Rect(0, 20, selectedStateWidth, 3), "");
            GUI.color = Color.white;
            GUILayout.EndVertical();

            #region Fields Contained in Selected State
            GUILayout.BeginHorizontal();

            Color32 nullSlotColour = new Color32(212, 17, 89, 255);
            Color32 fullSlotColour = Color.white;

            #region Action fields
            scrollActions = EditorGUILayout.BeginScrollView(scrollActions, GUILayout.Width(200), GUILayout.Height(200));
            SerializedObject _selectedState = new SerializedObject(selected);
            _selectedState.Update();
            SerializedProperty actions = _selectedState.FindProperty(nameof(selected.actions));
            for (int i = 0; i < actions.arraySize; i++)
            {
                SerializedProperty actionProp = actions.GetArrayElementAtIndex(i);
                GUI.color = actionProp.objectReferenceValue == null ? nullSlotColour : fullSlotColour;
                EditorGUILayout.PropertyField(actionProp, GUIContent.none);
            }
            _selectedState.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Connection fields + linked states
            scrollConnections = EditorGUILayout.BeginScrollView(scrollActions, GUILayout.Width(400), GUILayout.Height(200));
            _selectedState.Update();
            SerializedProperty connections = _selectedState.FindProperty(nameof(selected.connections));
            for (int i = 0; i < connections.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty connectionProp = connections.GetArrayElementAtIndex(i);
                GUI.color = connectionProp.objectReferenceValue == null ? nullSlotColour : fullSlotColour;
                EditorGUILayout.PropertyField(connectionProp, GUIContent.none);

                Connection currentConnection = (Connection)connectionProp.objectReferenceValue;
                if (currentConnection == null)
                {
                    GUI.color = nullSlotColour;
                    EditorGUILayout.LabelField("missing connection!!!", EditorStyles.whiteLargeLabel);
                    GUI.color = fullSlotColour;
                }
                else
                {
                    SerializedObject connectionSerializedObject = new SerializedObject(currentConnection);
                    SerializedProperty nextStateProp = connectionSerializedObject.FindProperty(nameof(currentConnection.nextState));
                    GUI.color = nextStateProp.objectReferenceValue == null ? nullSlotColour : fullSlotColour;
                    EditorGUILayout.PropertyField(nextStateProp, GUIContent.none);
                    connectionSerializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndHorizontal();
            }
            _selectedState.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            #endregion

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndArea();
            #endregion selected state column

            searchResults = Search(); // update search result filter tags to follow any changes made
        }

        Texture2D MakeTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }


        Dictionary<State, SearchFilter> Search()
        {
            var guidArray = AssetDatabase.FindAssets($"t:{nameof(State)}");
            List<string> guidList = guidArray.ToList<string>();

            var states = new List<State>();

            Dictionary<State, SearchFilter> statesWithFlags = new Dictionary<State, SearchFilter>();

            foreach (var guid in guidList)
            {
                var currentAsset = (State)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(State));

                // get rid of the source files
                if (
                    currentAsset.name == "State" ||
                    currentAsset.name == "Action" ||
                    currentAsset.name == "Connection"
                    )
                {
                    guidList.Remove(guid);
                    continue; // skip this iteration
                }

                SearchFilter flags = 0;

                // if (currentAsset.actions.Any())
                //     flags |= SearchFilter.Actions;

                // if (currentAsset.connections.Any())
                //     flags |= SearchFilter.Connections;

                foreach (var action in currentAsset.actions)
                {
                    if (action == null)
                    {
                        flags |= SearchFilter.MissingAction;
                        break;
                    }
                }

                foreach (var connection in currentAsset.connections)
                {
                    if (connection == null)
                    {
                        flags |= SearchFilter.MissingConnection;
                        flags |= SearchFilter.IncompleteConnection; // gets this one too by default
                    }
                    else if (connection.nextState == null)
                    {
                        flags |= SearchFilter.IncompleteConnection;
                    }
                }

                statesWithFlags.Add(currentAsset, flags);
            }

            return statesWithFlags;
        }

        // kind of hacky repainter loop so that search result button hover updates faster
        async void Repainter()
        {
            while (repaint)
            {
                this.Repaint();
                await Task.Delay(100);
            }
        }

        private void OnDisable()
        {
            showResults = false;
            repaint = false;
        }

        private void Awake()
        {
            repaint = true;
            Repainter();
        }
    }
}

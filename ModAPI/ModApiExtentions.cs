﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HutongGames.PlayMaker;
using MSCLoader;

using TommoJProductions.ModApi.Attachable;
using TommoJProductions.ModApi.PlaymakerExtentions;
using UnityEngine;

using static UnityEngine.GUILayout;
using static TommoJProductions.ModApi.ModClient;
using HutongGames.PlayMaker.Actions;

namespace TommoJProductions.ModApi
{
    /// <summary>
    /// Represents common extention methods. 
    /// </summary>
    public static class ModApiExtentions
    {
        #region Coroutine stuff

        /// <summary>
        /// Runs a coroutine synchronously. Waits for the coroutine to finish.
        /// </summary>
        /// <param name="func">The corountine to run synchronously.</param>
        public static void waitCoroutine(this IEnumerator func)
        {
            while (func.MoveNext())
            {
                if (func.Current != null)
                {
                    if (func.Current is WaitForSeconds)
                    {
                        Debug.LogWarning($"[WaitForCoroutine] Skipped call to WaitForSeconds.");
                        continue;
                    }
                    waitCoroutine((IEnumerator)func.Current);
                }
            }
        }

        #endregion

        #region ExMethods

        /// <summary>
        /// returns true if <paramref name="integer"/> is even.
        /// </summary>
        /// <param name="integer"></param>
        /// <returns></returns>
        public static bool isEven(this int integer)
        {
            // Written, 03.08.2022

            return integer % 2 == 0;
        }
        /// <summary>
        /// Teleports a part to a world position.
        /// </summary>
        /// <param name="part">The part to teleport.</param>
        /// <param name="force">force a uninstall if required?</param>
        /// <param name="position">The position to teleport the part to.</param>
        public static void teleport(this Part part, bool force, Vector3 position)
        {
            // Written, 09.07.2022

            if (part.installed && force)
                part.disassemble(true);
            part.transform.teleport(position);
        }
        /// <summary>
        /// Teleports a gameobject to a world position.
        /// </summary>
        /// <param name="gameobject">The gameobject to teleport</param>
        /// <param name="position">The position to teleport the go to.</param>
        public static void teleport(this GameObject gameobject, Vector3 position)
        {
            // Written, 09.07.2022

            gameobject.transform.teleport(position);
        }
        /// <summary>
        /// Teleports a transform to a world position.
        /// </summary>
        /// <param name="transform">The transform to teleport</param>
        /// <param name="position">The position to teleport the go to.</param>
        public static void teleport(this Transform transform, Vector3 position)
        {
            // Written, 09.07.2022

            Rigidbody rb = transform.GetComponent<Rigidbody>();
            if (rb)
                if (!rb.isKinematic)
                    rb = null;
                else
                    rb.isKinematic = true;
            transform.root.position = position;
            if (rb)
                rb.isKinematic = false;
        }

        /// <summary>
        /// [GUI] draws a property. eg => <see cref="Part.partSettings"/>.drawProperty("assembleType") would draw a property as such: "assembleType: joint".
        /// </summary>
        /// <param name="t">The class instance get value from.</param>
        /// <param name="memberName">The class member name to get a field/property instance from.</param>
        public static void drawProperty<T>(this T t, string memberName) where T : class
        {
            // Written, 08.07.2022

            Type _t = t.GetType();
            FieldInfo fi = _t.GetField(memberName);
            PropertyInfo pi = null;
            if (fi == null)
                pi = _t.GetProperty(memberName);
            string title = "null";
            object value = "null";
            if (_t.IsEnum)
            {
                title = _t.Name;
                value = _t.GetField(memberName).GetValue(t);
            }
            if (fi != null)
            {
                title = fi.Name;
                value = fi.GetValue(t);
            }
            else if (pi != null)
            {
                title = pi.Name;

                if (pi.CanRead)
                {
                    value = pi.GetValue(t, null);
                }
            }
            Label($"{title}: {value}");
        }
        /// <summary>
        /// [GUI] draws an enum as a property.
        /// </summary>
        /// <param name="t">The enum instance get value from.</param>
        public static void drawProperty<T>(this T t) where T : Enum
        {
            // Written, 08.07.2022

            Type _t = t.GetType();
            string title = _t.Name;
            string valueName = t.ToString();
            string value = _t.GetField(valueName)?.GetValue(t).ToString();
            Label($"{title}: {value ?? valueName}");
        }

        /// <summary>
        /// gets the actual size of the mesh. based off mesh bounds and transform scale
        /// </summary>
        /// <param name="filter">the mesh filter to get mesh and scale.</param>
        /// <returns>filter.mesh size * transform scale.</returns>
        public static Vector3 getMeshSize(this MeshFilter filter)
        {
            // Written, 15.07.2022

            Vector3 size = filter.mesh.bounds.size;
            Vector3 scale = filter.transform.localScale;
            return new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
        }
        /// <summary>
        /// gets the actual size of the mesh. based off mesh bounds and transform scale
        /// </summary>
        /// <param name="transform">the transform that this mesh is on.</param>
        /// <param name="mesh">the mesh to get size of</param>
        /// <returns>mesh size * transform scale.</returns>
        public static Vector3 getMeshSize(this Transform transform, Mesh mesh)
        {
            // Written, 15.07.2022

            Vector3 size = mesh.bounds.size;
            Vector3 scale = transform.localScale;
            return new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
        }

        /// <summary>
        /// loads data from a file or throws an error on if save data doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of Object to load</typeparam>
        /// <param name="data">The ref to load data to</param>
        /// <param name="mod">The mod to save data on</param>
        /// <param name="saveFileName">The save file name with or without file extention. </param>
        /// <exception cref="saveDataNotFoundException"/>
        public static void loadDataOrThrow<T>(this Mod mod, out T data, string saveFileName) where T : class, new()
        {
            // Written, 15.07.2022

            if (File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(mod), saveFileName)))
            {
                data = MSCLoader.SaveLoad.DeserializeSaveFile<T>(mod, saveFileName);
                print($"{mod.ID}: loaded save data (Exists).");
            }
            else
            {
                string error = $"{mod.ID}: save file didn't exist.. throwing saveDataNotFoundException.";
                print(error);
                throw new saveDataNotFoundException(error);
            }
        }
        /// <summary>
        /// saves data to a file.
        /// </summary>
        /// <typeparam name="T">The type of Object to save</typeparam>
        /// <param name="data">the data to save.</param>
        /// <param name="mod">The mods config folder to save data in</param>
        /// <param name="saveFileName">The save file name with or without file extention. </param>
        /// <returns>Returns <see langword="true"/> if save file exists.</returns>
        public static void saveData<T>(this Mod mod, T data, string saveFileName) where T : class, new()
        {
            // Written, 12.06.2022

            MSCLoader.SaveLoad.SerializeSaveFile(mod, data, saveFileName);
            print($"{mod.ID}: saved data.");
        }
        /// <summary>
        /// Loads data from a file. if data doesn't exist, creates new data.
        /// </summary>
        /// <typeparam name="T">The type of Object to load</typeparam>
        /// <param name="data">The ref to load data to</param>
        /// <param name="mod">The mod to save data on</param>
        /// <param name="saveFileName">The save file name with or without file extention. </param>
        /// <returns>Returns <see langword="true"/> if save file exists.</returns>
        public static bool loadOrCreateData<T>(this Mod mod, out T data, string saveFileName) where T : class, new()
        {
            // Written, 12.06.2022

            if (File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(mod), saveFileName)))
            {
                data = MSCLoader.SaveLoad.DeserializeSaveFile<T>(mod, saveFileName);
                print($"{mod.ID}: loaded save data (Exists).");
                return true;
            }
            else
            {
                print($"{mod.ID}: save file didn't exist.. creating a save file.");
                data = new T();
                return false;
            }
        }


        /// <summary>
        /// executes an action on all objects of an array.
        /// </summary>
        /// <typeparam name="T">The type of array</typeparam>
        /// <param name="objects">the instance of the array</param>
        /// <param name="func">the function to perform on all elements of the array.</param>
        public static void forEach<T>(this T[] objects, Action<T> func) where T : class
        {
            // Written, 11.07.2022

            for (int i = 0; i < objects.Count(); i++)
            {
                func.Invoke(objects[i]);
            }
        }
        /// <summary>
        /// Gets the first object in array that meets <paramref name="func"/>
        /// </summary>
        /// <typeparam name="T">The Class to find</typeparam>
        /// <param name="objects">The array to find an object in.</param>
        /// <param name="func">A function of type T.</param>
        public static T first<T>(this T[] objects, Func<T, bool> func) where T : class
        {
            // Written, 11.07.2022

            for (int i = 0; i < objects.Length; i++)
            {
                if (func.Invoke(objects[i]))
                {
                    return objects[i];
                }
            }
            return null;
        }
        /// <summary>
        /// Gets the first object in array that meets <paramref name="func"/>
        /// </summary>
        /// <typeparam name="T">The Class to find</typeparam>
        /// <param name="objects">The array to find an object in.</param>
        /// <param name="func">A function of type T.</param>
        public static T first<T>(this IEnumerable<T> objects, Func<T, bool> func) where T : class
        {
            // Written, 23.09.2023

            IEnumerator<T> e = objects.GetEnumerator();

            while (e.MoveNext())
            {
                if (func.Invoke((T)e.Current))
                    return e.Current as T;
            }
            return null;
        }
        /// <summary>
        /// Gets the first object in array that meets <paramref name="func"/>
        /// </summary>
        /// <typeparam name="T">The Class to find</typeparam>
        /// <param name="objects">The array to find an object in.</param>
        /// <param name="func">A function of type T.</param>
        public static T first<T>(this IEnumerable<T> objects, Func<T, bool> func, out int index) where T : class
        {
            // Written, 23.09.2023

            IEnumerator<T> e = objects.GetEnumerator();
            int i = 0;
            while (e.MoveNext())
            {
                if (func.Invoke((T)e.Current))
                {
                    index = i;
                    return e.Current as T;
                }
                i++;
            }
            index = -1;
            return null;
        }
        /// <summary>
        /// Gets the index of the first object in array that meets <paramref name="func"/> if a match isn't found, returns -1.
        /// </summary>
        /// <typeparam name="T">The Class to find</typeparam>
        /// <param name="objects">The array to find an object index in.</param>
        /// <param name="func">A function of type T.</param>
        /// <returns>The index of the first found object in the array</returns>
        public static int indexOf<T>(this T[] objects, Func<T, bool> func) where T : class
        {
            // Written, 11.07.2022

            for (int i = 0; i < objects.Length; i++)
            {
                if (func.Invoke(objects[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Gets the index of the first object in array that meets <paramref name="func"/> if a match isn't found, returns -1.
        /// </summary>
        /// <typeparam name="T">The Class to find</typeparam>
        /// <param name="objects">The array to find an object index in.</param>
        /// <param name="func">A function of type T.</param>
        /// <returns>The index of the first found object in the array</returns>
        public static int indexOf<T>(this IEnumerable<T> objects, Func<T, bool> func) where T : class
        {
            // Written, 23.09.2023

            IEnumerator<T> e = objects.GetEnumerator();
            int index = 0;
            while (e.MoveNext())
            {
                if (func.Invoke((T)e.Current))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Gets the first found behaviour in the parent. where <paramref name="func"/> action returns true.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <param name="func">the function to check with.</param>
        /// <returns></returns>
        public static T getBehaviourInParent<T>(this GameObject gameObject, Func<T, bool> func) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            Transform parent = gameObject.transform.parent;
            if (parent)
            {
                T t = parent.GetComponent<T>();
                if (t && func.Invoke(t))
                {
                    return t;
                }
                return parent.gameObject.getBehaviourInParent(func);
            }
            return null;
        }
        /// <summary>
        /// Gets the first found behaviour in the parent.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <returns></returns>
        public static T getBehaviourInParent<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            return getBehaviourInParent<T>(gameObject, func => true);
        }
        /// <summary>
        /// Gets all behaviours found in all parents. where <paramref name="func"/> action returns true.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <param name="func">the function to check with.</param>
        /// <returns></returns>
        public static T[] getBehavioursInParent<T>(this GameObject gameObject, Func<T, bool> func) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            List<T> values = new List<T>();
            Transform parent = gameObject.transform.parent;
            if (parent)
            {
                T t = parent.GetComponent<T>();
                if (t && func.Invoke(t))
                {
                    values.Add(t);
                }
                T[] v = parent.gameObject.getBehavioursInParent(func);
                if (v != null && v.Length > 0)
                    values.AddRange(v);
            }
            if (values.Count == 0)
                return null;
            return values.ToArray();
        }
        /// <summary>
        /// Gets all behaviours found in all parents.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <returns></returns>
        public static T[] getBehavioursInParent<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            return getBehavioursInParent<T>(gameObject, func => true);
        }
        /// <summary>
        /// Gets the first found behaviour in the children. where <paramref name="func"/> action returns true.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <param name="func">the function to check with.</param>
        /// <returns></returns>
        public static T getBehaviourInChildren<T>(this GameObject gameObject, Func<T, bool> func) where T : MonoBehaviour
        {
            // Written, 11.07.2022

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                T t = child.GetComponent<T>();
                if (t && func.Invoke(t))
                {
                    return t;
                }
                T tc = child.gameObject.getBehaviourInChildren(func);
                if (tc != null)
                {
                    return tc;
                }             
            }
            return null;
        }
        /// <summary>
        /// Gets the first found behaviour in the children.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        public static T getBehaviourInChildren<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            // Written, 11.07.2022

            return getBehaviourInChildren<T>(gameObject, func => true);
        }

        /// <summary>
        /// Gets all behaviours found in all children. where <paramref name="func"/> action returns true.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <param name="func">the function to check with.</param>
        /// <returns></returns>
        public static T[] getBehavioursInChildren<T>(this GameObject gameObject, Func<T, bool> func) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            if (gameObject.transform.childCount > 0)
            {
                List<T> values = new List<T>();
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    Transform child = gameObject.transform.GetChild(i);
                    T t = child.GetComponent<T>();
                    if (t && func.Invoke(t))
                    {
                        values.Add(t);
                    }
                    T[] v = child.gameObject.getBehavioursInChildren(func);
                    if (v != null && v.Length > 0)
                        values.AddRange(v);
                }
                if (values.Count == 0)
                    return null;
                return values.ToArray();
            }
            return null;
        }
        /// <summary>
        /// Gets all behaviours found in all children. where <paramref name="func"/> action returns true.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        /// <param name="func">the function to check with.</param>
        /// <returns></returns>
        public static IEnumerator<T> getBehavioursInChildrenAsync<T>(this GameObject gameObject, Func<T, bool> func) where T : MonoBehaviour
        {
            // Written, 02.07.2022 

            if (gameObject.transform.childCount > 0)
            {
                List<T> values = new List<T>();
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    Transform child = gameObject.transform.GetChild(i);
                    T t = child.GetComponent<T>();
                    if (t && func.Invoke(t))
                    {
                        yield return t;
                        values.Add(t);
                    }
                    IEnumerator<T> s = child.gameObject.getBehavioursInChildrenAsync(func);
                    while (s.MoveNext())
                        yield return s.Current;
                }
            }
        }

        /// <summary>
        /// Gets all behaviours found in all children.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        public static T[] getBehavioursInChildren<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            return getBehavioursInChildren<T>(gameObject, func => true);
        }
        /// <summary>
        /// Gets all behaviours found in all children.
        /// </summary>
        /// <typeparam name="T">The type of behaviour to get</typeparam>
        /// <param name="gameObject">The gameobject to get the behaviour on.</param>
        public static IEnumerator<T> getBehavioursInChildrenAsync<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            // Written, 02.07.2022

            IEnumerator<T> s = getBehavioursInChildrenAsync<T>(gameObject, func => true);
            while (s.MoveNext())
            {
                yield return s.Current;
            }
        }

        /// <summary>
        /// returns whether the player is currently looking at an gameobject. By raycast.
        /// </summary>
        /// <param name="gameObject">The gameobject to detect againist.</param>
        /// <param name="maxDistance">raycast within distance from main camera.</param>
        public static bool isPlayerLookingAt(this GameObject gameObject, float maxDistance = 1)
        {
            if (raycast(out RaycastHit hit, maxDistance, false, (LayerMasksEnum)gameObject.layer))
            {
                if (hit.collider?.gameObject == gameObject)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// returns whether the player is currently looking at a part. By raycasting on <paramref name="part"/>.gameobject.layer. Does not abid by stock game logic. 
        /// Returns true when looking at a <see cref="Part"/> even if the player is in "ToolMode" or any other situation that prevents the player from being able to pickup objects.
        /// </summary>
        /// <param name="part">The part to detect againist.</param>
        /// <param name="maxDistance">raycast within distance from main camera.</param>
        public static bool isPlayerLookingAt(this Part part, float maxDistance = 1)
        {
            return part.gameObject.isPlayerLookingAt(maxDistance);
        }
        /// <summary>
        /// Checks if the player is looking at this gameobject. returns false if player is looking at nothing. by checking Players raycasthitgameobject FSM. Abids by stock game logic.
        /// Returns false if the player is in toolmode or any other situation that prevents the player from being able to pickup objects even if looking at a <see cref="Part"/>
        /// </summary>
        public static bool isPlayerLookingAtByPickUp(this GameObject go)
        {
            // Written, 08.05.2022

            return getRaycastHitGameObject?.Value == go;
        }
        /// <summary>
        /// Checks if the player is looking at this part. returns false if player is looking at nothing. by checking Players raycasthitgameobject FSM. Abids by stock game logic.
        /// Returns false if the player is in toolmode or any other situation that prevents the player from being able to pickup objects even if looking at a <see cref="Part"/>
        /// </summary>
        public static bool isPlayerLookingAtByPickUp(this Part part)
        {
            // Written, 08.05.2022

            return getRaycastHitGameObject?.Value == part.gameObject;
        }

        /// <summary>
        /// Checks if player is holding the GameObject, <paramref name="inGameObject"/>. by checking if <paramref name="inGameObject"/>'s <see cref="GameObject.layer"/> 
        /// equals <see cref="LayerMasksEnum.Wheel"/>.
        /// </summary>
        /// <param name="inGameObject">The gameObject to check on.</param>
        public static bool isPlayerHolding(this GameObject inGameObject)
        {
            // Written, 02.10.2018

            if (inGameObject.isOnLayer(LayerMasksEnum.Wheel))
                return true;
            return false;
        }
        /// <summary>
        /// Checks if player is holding the Part, <paramref name="part"/>. by checking if <paramref name="part"/>'s <see cref="GameObject.layer"/> equals <see cref="LayerMasksEnum.Wheel"/>
        /// </summary>
        /// <param name="part">The gameObject to check on.</param>
        public static bool isPlayerHolding(this Part part)
        {
            // Written, 26.08.2023

            return part.pickedUp;
        }
        /// <summary>
        /// checks if the player is holding this gameobject. returns false if player is holding nothing. by checking Players pickedupgameobject FSM
        /// </summary>
        public static bool isPlayerHoldingByPickup(this GameObject go)
        {
            // Written, 08.05.2022

            return getPickedUpGameObject?.Value == go;
        }

        /// <summary>
        /// Checks if the gameobject is on the layer, <paramref name="inLayer"/>.
        /// </summary>
        /// <param name="inGameObject">The gameobject to check layer.</param>
        /// <param name="inLayer">The layer to check for.</param>
        public static bool isOnLayer(this GameObject inGameObject, LayerMasksEnum inLayer)
        {
            // Written, 02.10.2018

            if (inGameObject.layer == inLayer.layer())
                return true;
            return false;
        }

        /// <summary>
        /// Sends all children of game object to layer.
        /// </summary>
        /// <param name="inGameObject">The gameobject to get children from.</param>
        /// <param name="inLayer">The layer to send gameobject children to.</param>
        public static void sendChildrenToLayer(this GameObject inGameObject, LayerMasksEnum inLayer)
        {
            // Written, 27.09.2018

            Transform transform = inGameObject.transform;

            if (transform != null)
                for (int i = 0; i < inGameObject.transform.childCount; i++)
                {
                    inGameObject.transform.GetChild(i).gameObject.sendToLayer(inLayer);
                }
        }
        /// <summary>
        /// Sends a gameobject to the desired layer.
        /// </summary>
        /// <param name="inGameObject">The gameObject.</param>
        /// <param name="inLayer">The Layer.</param>
        /// <param name="inSendAllChildren">[Optional] sends all children to the layer.</param>
        public static void sendToLayer(this GameObject inGameObject, LayerMasksEnum inLayer, bool inSendAllChildren = false)
        {
            // Written, 02.10.2018

            inGameObject.layer = layer(inLayer);
            if (inSendAllChildren)
                inGameObject.sendChildrenToLayer(inLayer);
        }
        /// <summary>
        /// Returns the layer index number.
        /// </summary>
        /// <param name="inLayer">The layer to get index.</param>
        public static int layer(this LayerMasksEnum inLayer)
        {
            // Written, 02.10.2018

            return (int)inLayer;
        }
        /// <summary>
        /// Returns the name of the layer.
        /// </summary>
        /// <param name="inLayer">The layer to get.</param>
        public static string name(this LayerMasksEnum inLayer)
        {
            // Written, 02.10.2018

            string layerName;
            switch (inLayer)
            {
                case LayerMasksEnum.Bolts:
                    layerName = "Bolts";
                    break;
                case LayerMasksEnum.Cloud:
                    layerName = "Cloud";
                    break;
                case LayerMasksEnum.Collider:
                    layerName = "Collider";
                    break;
                case LayerMasksEnum.Collider2:
                    layerName = "Collider2";
                    break;
                case LayerMasksEnum.Dashboard:
                    layerName = "Dashboard";
                    break;
                case LayerMasksEnum.Datsun:
                    layerName = "Datsun";
                    break;
                case LayerMasksEnum.Default:
                    layerName = "Default";
                    break;
                case LayerMasksEnum.DontCollide:
                    layerName = "DontCollide";
                    break;
                case LayerMasksEnum.Forest:
                    layerName = "Forest";
                    break;
                case LayerMasksEnum.Glass:
                    layerName = "Glass";
                    break;
                case LayerMasksEnum.GUI:
                    layerName = "GUI";
                    break;
                case LayerMasksEnum.HingedObjects:
                    layerName = "HingedObjects";
                    break;
                case LayerMasksEnum.IgnoreRaycast:
                    layerName = "IgnoreRaycast";
                    break;
                case LayerMasksEnum.Lifter:
                    layerName = "Lifter";
                    break;
                case LayerMasksEnum.NoRain:
                    layerName = "NoRain";
                    break;
                case LayerMasksEnum.Parts:
                    layerName = "Parts";
                    break;
                case LayerMasksEnum.Player:
                    layerName = "Player";
                    break;
                case LayerMasksEnum.PlayerOnlyColl:
                    layerName = "PlayerOnlyColl";
                    break;
                case LayerMasksEnum.Road:
                    layerName = "Road";
                    break;
                case LayerMasksEnum.Terrain:
                    layerName = "Terrain";
                    break;
                case LayerMasksEnum.Tools:
                    layerName = "Tools";
                    break;
                case LayerMasksEnum.TransparentFX:
                    layerName = "TransparentFX";
                    break;
                case LayerMasksEnum.TriggerOnly:
                    layerName = "TriggerOnly";
                    break;
                case LayerMasksEnum.UI:
                    layerName = "UI";
                    break;
                case LayerMasksEnum.Water:
                    layerName = "Water";
                    break;
                case LayerMasksEnum.Wheel:
                    layerName = "Wheel";
                    break;
                default:
                    layerName = "Blank";
                    break;
            }
            return layerName;
        }

        /// <summary>
        /// Creates and returns a new rigidbody on the gameobject and assigns parameters listed.
        /// </summary>
        /// <param name="gameObject">The gameobject to create a rigidbody on.</param>
        /// <param name="rigidbodyConstraints">Create rigidbody with these constraints.</param>
        /// <param name="mass">The mass of the rb.</param>
        /// <param name="drag">The drag of the rb</param>
        /// <param name="angularDrag">The angular drag of the rb.</param>
        /// <param name="isKinematic">is this rigidbody kinmatic?</param>
        /// <param name="useGravity">is this rigidbody affected by gravity?</param>
        public static Rigidbody createRigidbody(this GameObject gameObject, RigidbodyConstraints rigidbodyConstraints = RigidbodyConstraints.None, float mass = 1,
            float drag = 0, float angularDrag = 0.05f, bool isKinematic = false, bool useGravity = true)
        {
            // Written, 10.09.2021

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = rigidbodyConstraints;
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;
            return rb;
        }
        /// <summary>
        /// Fixes a transform to another transform.
        /// </summary>
        /// <param name="transform">The transform to fix</param>
        /// <param name="parent">The parent transform to fix <paramref name="transform"/></param>
        public static IEnumerator fixToParent(this Transform transform, Transform parent)
        {
            while (transform.parent != parent)
            {
                transform.parent = parent;
                yield return null;
            }
        }
        /// <summary>
        /// Sets its position and rotation.
        /// </summary>
        /// <param name="transform">The transform to fix</param>
        /// <param name="pos">The pos to set</param>
        /// <param name="eulerAngles">the rot to set</param>
        public static IEnumerator fixTransform(this Transform transform, Vector3 pos, Vector3 eulerAngles)
        {
            while (transform.localPosition != pos && transform.localEulerAngles != eulerAngles)
            {
                transform.localPosition = pos;
                transform.localEulerAngles = eulerAngles;
                yield return null;
            }
        }

        private static FsmStateActionCallback determineAndCreateCallbackType(CallbackTypeEnum type, Action action, bool everyFrame = false)
        {
            // Written, 13.06.2022

            switch (type)
            {
                case CallbackTypeEnum.onFixedUpdate:
                    return new OnFixedUpdateCallback(action, everyFrame);
                case CallbackTypeEnum.onUpdate:
                    return new OnUpdateCallback(action, everyFrame);
                case CallbackTypeEnum.onGui:
                    return new OnGuiCallback(action, everyFrame);
                default:
                case CallbackTypeEnum.onEnter:
                    return new OnEnterCallback(action, everyFrame);
            }
        }
        private static FsmStateActionCallback determineAndCreateCallbackType(CallbackTypeEnum type, Func<bool> func, bool everyFrame = false)
        {
            // Written, 13.06.2022

            switch (type)
            {
                case CallbackTypeEnum.onFixedUpdate:
                    return new OnFixedUpdateCallback(func, everyFrame);
                case CallbackTypeEnum.onUpdate:
                    return new OnUpdateCallback(func, everyFrame);
                case CallbackTypeEnum.onGui:
                    return new OnGuiCallback(func, everyFrame);
                default:
                case CallbackTypeEnum.onEnter:
                    return new OnEnterCallback(func, everyFrame);
            }
        }
        private static FsmStateActionCallback determineAndCreateActionType(this FsmState state, InjectEnum injectType, Action action, CallbackTypeEnum callbackType, bool everyFrame, int index = 0)
        {
            // Written, 18.06.2022

            FsmStateActionCallback cb;
            switch (injectType)
            {
                case InjectEnum.prepend:
                    cb = state.prependNewAction(action, callbackType, everyFrame);
                    break;
                case InjectEnum.insert:
                    cb = state.insertNewAction(action, index, callbackType, everyFrame);
                    break;
                case InjectEnum.replace:
                    cb = state.replaceAction(action, index, callbackType, everyFrame);
                    break;
                default:
                case InjectEnum.append:
                    cb = state.appendNewAction(action, callbackType, everyFrame);
                    break;
            }
            return cb;
        }

        /// <summary>
        /// adds a new global transition.
        /// </summary>
        /// <param name="fsm"></param>
        /// <param name="_event"></param>
        /// <param name="toStateName"></param>
        public static FsmTransition addNewGlobalTransition(this PlayMakerFSM fsm, FsmEvent _event, string toStateName)
        {
            FsmTransition[] fsmGlobalTransitions = fsm.FsmGlobalTransitions;
            List<FsmTransition> temp = new List<FsmTransition>();
            foreach (FsmTransition t in fsmGlobalTransitions)
            {
                temp.Add(t);
            }
            FsmTransition ft = new FsmTransition
            {
                FsmEvent = _event,
                ToState = toStateName
            };
            temp.Add(ft);
            fsm.Fsm.GlobalTransitions = temp.ToArray();
            print($"Adding new global transition, to fsm, {fsm.gameObject.name}.{fsm.name}. {_event.Name} transitions this fsm state to {toStateName}");
            return ft;
        }
        /// <summary>
        /// Adds a new local transition.
        /// </summary>
        /// <param name="state">The state to add a transition.</param>
        /// <param name="eventName">The event that triggers this transition.</param>
        /// <param name="toStateName">the state it should change to when transition is triggered.</param>
        public static FsmTransition addNewTransitionToState(this FsmState state, string eventName, string toStateName)
        {
            List<FsmTransition> temp = new List<FsmTransition>();
            foreach (FsmTransition t in state.Transitions)
            {
                temp.Add(t);
            }
            FsmTransition ft = new FsmTransition
            {
                FsmEvent = state.Fsm.GetEvent(eventName),
                ToState = toStateName
            };
            temp.Add(ft);
            state.Transitions = temp.ToArray();
            print($"Adding new local transition, to state, {state.Fsm.GameObject.name}.{state.Fsm.Name}.{state.Name}. {eventName} transitions this fsm from {state.Name} to {toStateName}");
            return ft;
        }

        /// <summary>
        /// Appends an action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="action">the action to append.</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback appendNewAction(this FsmState state, Action action, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.appendAction(determineAndCreateCallbackType(callbackType, action, everyFrame));
        }
        /// <summary>
        /// Appends an action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="func">the action to append.</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback appendNewAction(this FsmState state, Func<bool> func, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.appendAction(determineAndCreateCallbackType(callbackType, func, everyFrame));
        }
        /// <summary>
        /// Appends an enumerator in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="routine">the enumerator to append.</param>
        public static FsmStateActionCallback appendNewAction(this FsmState state, IEnumerator routine)
        {
            // Written, 18.11.2022

            return appendNewAction(state, () => { state.Fsm.Owner.StartCoroutine(routine); });
        }
        private static FsmStateActionCallback appendAction(this FsmState state, FsmStateActionCallback callback)
        {
            // Written, 13.04.2022

            List<FsmStateAction> temp = new List<FsmStateAction>();
            foreach (FsmStateAction v in state.Actions)
            {
                temp.Add(v);
            }
            temp.Add(callback);
            state.Actions = temp.ToArray();
            print($"[{callback.action.Method.DeclaringType.Name}] Appending new action {callback.action.Method.DeclaringType.Name}.{callback.action.Method.Name} to {state.Fsm.GameObject.name}.{state.Fsm.Name}.{state.Name}. Called {callback.callbackType} {(callback.everyFrame ? "every frame" : "once")}.");
            return callback;
        }


        /// <summary>
        /// prepends an action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="action">the action to prepend.</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback prependNewAction(this FsmState state, Action action, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.prependAction(determineAndCreateCallbackType(callbackType, action, everyFrame));
        }
        /// <summary>
        /// prepends an action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="func">the func to prepend. the func should return if fsmAction should finish.</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback prependNewAction(this FsmState state, Func<bool> func, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.prependAction(determineAndCreateCallbackType(callbackType, func, everyFrame));
        }
        /// <summary>
        /// prepends an enumerator in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="routine">the routine to prepend.</param>
        public static FsmStateActionCallback prependNewAction(this FsmState state, IEnumerator routine)
        {
            // Written, 18.11.2022

            return prependNewAction(state, () => { state.Fsm.Owner.StartCoroutine(routine); });
        }
        private static FsmStateActionCallback prependAction(this FsmState state, FsmStateActionCallback callback)
        {
            // Written, 13.04.2022

            List<FsmStateAction> temp = new List<FsmStateAction>();
            temp.Add(callback);
            foreach (FsmStateAction v in state.Actions)
            {
                temp.Add(v);
            }
            state.Actions = temp.ToArray();
            print($"[{callback.action.Method.DeclaringType.Name}] Prepending new {callback.action.Method.DeclaringType.Name}.{callback.action.Method.Name} to {state.Fsm.GameObject.name}.{state.Fsm.Name}.{state.Name}. Called {(callback.everyFrame ? "every frame" : "once")}.");
            return callback;
        }

        /// <summary>
        /// Inserts a new action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="action">the action to insert.</param>
        /// <param name="index">the index to insert action</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback insertNewAction(this FsmState state, Action action, int index, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.insertAction(determineAndCreateCallbackType(callbackType, action, everyFrame), index);
        }
        /// <summary>
        /// Inserts a new func in a state. return true if you would like the FsmAction to finish. (if true invokes, <see cref="FsmStateAction.Finish()"/>)
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="func">the action to insert.</param>
        /// <param name="index">the index to insert action</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback insertNewAction(this FsmState state, Func<bool> func, int index, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.insertAction(determineAndCreateCallbackType(callbackType, func, everyFrame), index);
        }
        /// <summary>
        /// Inserts an enumerator in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="routine">the enumerator to insert.</param>
        /// <param name="index">the index to insert action</param>
        public static FsmStateActionCallback insertNewAction(this FsmState state, IEnumerator routine, int index)
        {
            // Written, 18.11.2022

            return insertNewAction(state, () => { state.Fsm.Owner.StartCoroutine(routine); }, index);
        }
        private static FsmStateActionCallback insertAction(this FsmState state, FsmStateActionCallback callback, int index)
        {
            // Written, 13.04.2022

            List<FsmStateAction> temp = new List<FsmStateAction>();
            foreach (FsmStateAction v in state.Actions)
            {
                temp.Add(v);
            }
            temp.Insert(index, callback);
            state.Actions = temp.ToArray();
            print($"[{callback.action.Method.DeclaringType.Name}] Inserting new {callback.action.Method.DeclaringType.Name}.{callback.action.Method.Name} into {state.Fsm.GameObject.name}.{state.Fsm.Name}.{state.Name} at index:{index}. Called {(callback.everyFrame ? "every frame" : "once")}.");
            return callback;
        }

        /// <summary>
        /// Replaces an action in a state.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="action">the action to replace.</param>
        /// <param name="index">the index to replace action</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback replaceAction(this FsmState state, Action action, int index, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.replaceAction(determineAndCreateCallbackType(callbackType, action, everyFrame), index);
        }
        /// <summary>
        /// Replaces an func in a state. return true if you would like the FsmAction to finish. (if true invokes, <see cref="FsmStateAction.Finish()"/>)
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="func">the action to replace.</param>
        /// <param name="index">the index to replace action</param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback replaceAction(this FsmState state, Func<bool> func, int index, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            // Written, 07.12.2022

            return state.replaceAction(determineAndCreateCallbackType(callbackType, func, everyFrame), index);
        }
        /// <summary>
        /// Replaces an action in a state with an enumerator.
        /// </summary>
        /// <param name="state">the sate to modify.</param>
        /// <param name="routine">the enumerator to replace.</param>
        /// <param name="index">the index to replace action</param>
        public static FsmStateActionCallback replaceAction(this FsmState state, IEnumerator routine, int index)
        {
            // Written, 18.11.2022

            return replaceAction(state, () => { state.Fsm.Owner.StartCoroutine(routine); }, index);
        }
        private static FsmStateActionCallback replaceAction(this FsmState state, FsmStateActionCallback callback, int index)
        {
            // Written, 13.04.2022

            List<FsmStateAction> temp = new List<FsmStateAction>();
            for (int i = 0; i < state.Actions.Length; i++)
            {
                if (i == index)
                    temp.Add(callback);
                else
                    temp.Add(state.Actions[i]);
            }
            state.Actions = temp.ToArray();
            print($"[{callback.action.Method.DeclaringType.Name}] Replacing {callback.action.Method.DeclaringType.Name}.{callback.action.Method.Name} with {state.Fsm.GameObject.name}.{state.Fsm.Name}.{state.Name} at index:{index}. Called {(callback.everyFrame ? "every frame" : "once")}.");
            return callback;
        }

        /// <summary>
        /// Represents inject action logic.
        /// </summary>
        /// <param name="playMakerFSM">The playermaker to search on.</param>
        /// <param name="stateName">the state name to inject on.</param>
        /// <param name="injectType">inject type.</param>
        /// <param name="callback">inject callback</param>
        /// <param name="index">index to inject at. NOTE: only applies to <see cref="InjectEnum.insert"/> and <see cref="InjectEnum.replace"/></param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback injectAction(this PlayMakerFSM playMakerFSM, string stateName, InjectEnum injectType, Action callback, int index = 0, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            return injectAction(playMakerFSM.gameObject, playMakerFSM.FsmName, stateName, injectType, callback, index, callbackType, everyFrame);
        }
        /// <summary>
        /// Represents inject action logic.
        /// </summary>
        /// <param name="go">The gamobject to search on.</param>
        /// <param name="fsmName">the playmaker fsm name to search on</param>
        /// <param name="stateName">the state name to inject on.</param>
        /// <param name="injectType">inject type.</param>
        /// <param name="callback">inject callback</param>
        /// <param name="index">index to inject at. NOTE: only applies to <see cref="InjectEnum.insert"/> and <see cref="InjectEnum.replace"/></param>
        /// <param name="callbackType">Represents the callback type.</param>
        /// <param name="everyFrame">Represents if it should run everyframe..</param>
        public static FsmStateActionCallback injectAction(this GameObject go, string fsmName, string stateName, InjectEnum injectType, Action callback, int index = 0, CallbackTypeEnum callbackType = 0, bool everyFrame = false)
        {
            PlayMakerFSM fsm = go.GetPlayMaker(fsmName);
            FsmState state = go.GetPlayMakerState(stateName);
            return state.determineAndCreateActionType(injectType, callback, callbackType, everyFrame, index);
        }

        /// <summary>
        /// Rounds to <paramref name="decimalPlace"/>
        /// </summary>
        /// <param name="fsmFloat">The fsmFloat to round</param>
        /// <param name="decimalPlace">how many decimal places. eg. 2 = 0.01</param>
        public static FsmFloat round(this FsmFloat fsmFloat, int decimalPlace = 0)
        {
            return fsmFloat.Value.round(decimalPlace);
        }
        /// <summary>
        /// Rounds to <paramref name="decimalPlace"/>
        /// </summary>
        /// <param name="_float">The float to round</param>
        /// <param name="decimalPlace">how many decimal places. eg. 2 = 0.01</param>
        public static float round(this float _float, int decimalPlace = 0)
        {
            // Written, 15.01.2022

            return (float)Math.Round(_float, decimalPlace);
        }
        /// <summary>
        /// Rounds to <paramref name="decimalPlace"/>
        /// </summary>
        /// <param name="_double">The float to round</param>
        /// <param name="decimalPlace">how many decimal places. eg. 2 = 0.01</param>
        public static double round(this double _double, int decimalPlace = 0)
        {
            // Written, 15.01.2022

            return Math.Round(_double, decimalPlace);
        }

        /// <summary>
        /// Maps a value and its range to another. eg. (v=1 min=0, max=2). could map to (v=2 min=1 max=3).
        /// </summary>
        /// <param name="mainValue">the value to map</param>
        /// <param name="inValueMin">value min</param>
        /// <param name="inValueMax">value max</param>
        /// <param name="outValueMin">result min</param>
        /// <param name="outValueMax">result max</param>
        public static float mapValue(this float mainValue, float inValueMin, float inValueMax, float outValueMin, float outValueMax)
        {
            return (mainValue - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
        }
        /// <summary>
        /// Converts a <see cref="Vector3Info"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="i">the vector3 info to convert</param>
        /// <returns>the vector3 info as a vector3.</returns>
        public static Vector3 toVector3(this Vector3Info i)
        {
            return i.vector3;
        }
        /// <summary>
        /// converts a vector3.
        /// </summary>
        /// <param name="v">the vector3 to convert.</param>
        /// <returns>new instance Vector3Info</returns>
        public static Vector3Info toInfo(this Vector3 v)
        {
            return new Vector3Info(v);
        }
        /// <summary>
        /// Creates a new vector3 from an old one.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 copy(this Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        /// <summary>
        /// De/Activates all bolts in an array.
        /// </summary>
        /// <param name="bolts">the bolt array</param>
        /// <param name="active">De/Activate?</param>
        public static void activateBolts(this Bolt[] bolts, bool active)
        {
            // Written, 28.10.2022

            bolts.activateBolts(bolt => active);
        }
        /// <summary>
        /// De/Activates all bolts in an array.
        /// </summary>
        /// <param name="bolts">the bolt array</param>
        /// <param name="func">the func that returns De/Activate</param>
        public static void activateBolts(this Bolt[] bolts, Func<Bolt, bool> func)
        {
            // Written, 28.10.2022

            bool active;

            if (bolts != null)
            {
                for (int i = 0; i < bolts.Length; i++)
                {
                    active = func.Invoke(bolts[i]);
                    if (!bolts[i].settings.activeWhenUninstalled || active)
                    {
                        bolts[i].activateBolt(active);
                    }
                }
            }
        }
        /// <summary>
        /// Handles ID creation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="id"></param>
        /// <param name="func"></param>
        public static void setID<T>(this IEnumerable<T> collection, ref string id, Func<T, bool> func)
        {
            // Written, 03.09.2023

            T[] clones = collection.Where(func)?.ToArray();
            if (clones != null && clones.Length > 0)
            {
                id += clones.Length.ToString("000");
            }
            else
            {
                id += "000";
            }
        }

        #endregion    
    }
}

using Firebase;

using Firebase.Database;
using Firebase.Unity.Editor;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Conventions :
// prefab should has childrenwith names of class parameters else text will not be setted
// other suggested changes can be done in OnEnable() of a scripe named "RoomController" cause the type traited is Room , the 'C' should be UpperCase
// using the automatic stored data in "info" parameter
namespace brinis
{
    public class ListingManager : MonoBehaviour
    {
        public string key;
        public static ListingManager instance;
        public static MonoBehaviour trustedObject;
       
       
        void Awake()
        {
            instance = this;
            trustedObject = this;
         // trustedObject.StartCoroutine(SubscribeManager.FetchKey());
        }
       
        


       
       
        public static void PutFormularToDatabase<T>(Transform formularHead, System.Object t)
        {
            if (!SubscribeManager.Subscribed())
            {
                Debug.LogError("you are not subscribed to the Listing Manager");
                return;
            }
            t =EasyCrudsManager.GetInfoAutomaticly<T>(formularHead,t);
            Save<T>((T)t);
        }

        public static void Save<T>(object t)
        {
            if (!SubscribeManager.Subscribed())
            {
                Debug.LogError("you are not subscribed to the Listing Manager");
                return;
            }
            string id = "" + t.GetType().GetField("id").GetValue(t);
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError("id is null for the object of type " + t.GetType());
                return;
            }
            FirebaseDatabase.DefaultInstance.GetReference(EasyCrudsManager.TableName<T>()).Child(t.GetType().ToString()[0] + id).SetRawJsonValueAsync(JsonConvert.SerializeObject(t));
        }

        public static void SyncTableFromDatabase<T>(Transform prefab)
        {
           
            if (EasyCrudsManager.allPrefabs == null)
            {
                EasyCrudsManager.allPrefabs = new Dictionary<string, Transform>();
            }
            if (!EasyCrudsManager.allPrefabs.ContainsKey(EasyCrudsManager.TableName<T>()))
            {
                EasyCrudsManager.allPrefabs.Add(EasyCrudsManager.TableName<T>(), prefab);
            }
            if (PlayerPrefs.HasKey(EasyCrudsManager.TableName<T>()))
            {
                brinis.EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()] =
                    
                    JsonConvert.DeserializeObject<Dictionary<string,object>>(
                    PlayerPrefs.GetString(EasyCrudsManager.TableName<T>()).Replace("\\", "")
                    );
                StringJsonToCanvas<T>(PlayerPrefs.GetString(EasyCrudsManager.TableName<T>()));
            }
            trustedObject.StartCoroutine(WaitForKeyThenSubScribe<T>());

        }

        static IEnumerator WaitForKeyThenSubScribe<T>()
        {
            Debug.LogWarning("WaitForKeyThenSubScribe " + typeof(T));
            yield return null;
            while (!SubscribeManager.Subscribed())
            {
                Debug.LogWarning("waiting  key at "+instance.name);
                yield return new WaitForSeconds(1);
            }
            FirebaseDatabase.DefaultInstance
     .GetReference(EasyCrudsManager.TableName<T>())
     .ValueChanged += HandleValueChanged<T>;

        }
        public static void SyncTableFromDatabaseWithConditions<T>(Transform prefab, string key, string value, string key2 = null, string value2 = null)
        {
            if (EasyCrudsManager.allPrefabs == null)
            {
                EasyCrudsManager.allPrefabs = new Dictionary<string, Transform>();
            }
            if (!EasyCrudsManager.allPrefabs.ContainsKey(EasyCrudsManager.TableName<T>()))
            {
                EasyCrudsManager.allPrefabs.Add(EasyCrudsManager.TableName<T>(), prefab);
            }
            //   Ticket t = new Ticket();


            if (key2 != null)
            {
                FirebaseDatabase.DefaultInstance
           .GetReference(EasyCrudsManager.TableName<T>()).OrderByChild(key2).EqualTo(value2).OrderByChild(key).EqualTo(value)
           .ValueChanged += HandleValueChanged<T>;
            }
            else
            {
                FirebaseDatabase.DefaultInstance
               .GetReference(EasyCrudsManager.TableName<T>()).OrderByChild(key).EqualTo(value)
               .ValueChanged += HandleValueChanged<T>;
            }
            //t.service
            /*if (typeof(T).(v.na()+"") != null)
            {
              if(T.GetField(v.GetType() + "").GetValue(t)==v)
                {

                }
            }*/
        }

        void OnDestroy()
        {
            //FirebaseDatabase.DefaultInstance.
            foreach (string k in EasyCrudsManager.allPrefabs.Keys)
            {
                FirebaseDatabase.DefaultInstance
              .GetReference(k)
              .ValueChanged -= HandleValueChanged<Type>;
            }

        }
        static void HandleValueChanged<T>(object sender, ValueChangedEventArgs args)
        {
           // Debug.Log("HandleValueChanged " + args.Snapshot.GetRawJsonValue());
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            PlayerPrefs.SetString(EasyCrudsManager.TableName<T>(),args.Snapshot.GetRawJsonValue());
            StringJsonToCanvas<T>(args.Snapshot.GetRawJsonValue());
            /*if (!EasyCrudsManager.allTables.ContainsKey(EasyCrudsManager.TableName<T>()))
                EasyCrudsManager.allTables.Add(EasyCrudsManager.TableName<T>(), null);

            EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()] = JsonConvert.DeserializeObject<Dictionary<string, T>>(args.Snapshot.GetRawJsonValue()).ToDictionary(k => k.Key, k => (object)k.Value);
            if (!SubscribeManager.Subscribed() && false)
            {
                Debug.LogError("you are not subscribed to the Listing Manager");
            }
            else
            {
                if (EasyCrudsManager.allPrefabs.ContainsKey(EasyCrudsManager.TableName<T>()))
                    trustedObject.StartCoroutine(EasyCrudsManager.ShowAll<T>(EasyCrudsManager.allPrefabs[EasyCrudsManager.TableName<T>()], EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()].ToDictionary(k => k.Key, k => (T)k.Value)));
            }*/
          
           
            // allTables[EasyCrudsManager.TableName<T>()] = allInfos.ToDictionary(k => k.Key, k => (object)k.Value);
            //RoutineHolder
            // Do something with the data in args.Snapshot
        }

        public static void StringJsonToCanvas<T>(string json)
        {
            //Debug.Log("string to canvas for " + json);
            if (string.IsNullOrWhiteSpace(json)) return;
            if (!EasyCrudsManager.allTables.ContainsKey(EasyCrudsManager.TableName<T>()))
                EasyCrudsManager.allTables.Add(EasyCrudsManager.TableName<T>(), null);

            EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()] = JsonConvert.DeserializeObject<Dictionary<string,T>>(json).ToDictionary(k => k.Key, k => (object)k.Value);
            if (!SubscribeManager.Subscribed())
            {
                Debug.LogError("you are not subscribed to the Listing Manager");
            }
            else
            {
                if (EasyCrudsManager.allPrefabs.ContainsKey(EasyCrudsManager.TableName<T>()))
                    trustedObject.StartCoroutine(EasyCrudsManager.ShowAll<T>(EasyCrudsManager.allPrefabs[EasyCrudsManager.TableName<T>()], EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()].ToDictionary(k => k.Key, k => (T)k.Value)));
                else
                    Debug.Log("no prefab for " + EasyCrudsManager.TableName<T>());
            }
        }
        static WaitForSeconds w = new WaitForSeconds(0.001f);

     

  
        public static Dictionary<string,T> GetWholeTable<T>()
        {
            if(EasyCrudsManager.allTables.ContainsKey(EasyCrudsManager.TableName<T>()))
            {
                return EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()].ToDictionary(k => k.Key, k => (T)k.Value);
            }else
            {
                trustedObject.StartCoroutine(WaitForKeyThenSubScribe<T>());
                return null;
            }
            
            //allTables[EasyCrudsManager.TableName<T>()] = allInfos.ToDictionary(k => k.Key, k => (object)k.Value);
        }
        public static T Load<T>(string id)
        {
            if (EasyCrudsManager.allTables.ContainsKey(EasyCrudsManager.TableName<T>()))
            {
                if (!EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()].ContainsKey(typeof(T).ToString()[0] + id)) return default(T);
                return (T)EasyCrudsManager.allTables[EasyCrudsManager.TableName<T>()][typeof(T).ToString()[0] + id];
            }
            else
            {
                trustedObject.StartCoroutine(WaitForKeyThenSubScribe<T>());
                return default(T);
            }
            
        }
      
        
}
}

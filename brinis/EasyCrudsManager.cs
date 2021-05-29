using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace brinis
{
    public class EasyCrudsManager : MonoBehaviour
    {
        public static Type[] typesString = new Type[] { typeof(string) };
        static string filename, filePath;
        public static EasyCrudsManager instance;
        public static Dictionary<string, Transform> allInstances = new Dictionary<string, Transform>();
        public static MonoBehaviour trustedObject;
        public static Dictionary<string, Transform> allPrefabs;
        public static Dictionary<string, Dictionary<string, object>> allTables = new Dictionary<string, Dictionary<string, object>>();
        static WaitForSeconds w = new WaitForSeconds(0.001f);
        public RawImage pivotRawImageForSprites;
        private void Awake()
        {
            instance = this;
            trustedObject = instance;
            //Debug.Log("wtf");
        }
        public Dictionary<string, T> ListToDictionary<T>(List<T> list)
        {
            Dictionary<string, T> dic = new System.Collections.Generic.Dictionary<string, T>();
            foreach (T t in list)
            {
                dic.Add("" + t.GetType().GetField("id").GetValue(t), t);
            }
            return dic;
        }

        public static IEnumerator ShowAll<T>(Transform prefab, Dictionary<string, T> allInfos)
        {

                if(trustedObject==null)
                {
                    trustedObject = FindObjectOfType<MonoBehaviour>();
                }
           
           
                /// allTables[TableName<T>()] = allInfos.ToDictionary();

                prefab.gameObject.SetActive(false);
                Type[] arguments = allInfos.GetType().GetGenericArguments();
                Type keyType = arguments[0];
                Type valueType = arguments[1];

                // Debug.Log(JsonConvert.SerializeObject(allInfos));
                // Debug.Log(JsonConvert.SerializeObject(allpropertys));

                float time = Time.realtimeSinceStartup;
                float frame = 1 / 60;
                allInfos = LastMoves<T>(allInfos);
                foreach (string k in allInfos.Keys)
                {
                    if (ShouldShow<T>(allInfos[k]))
                    {
                        //UnityMainThreadDispatcher.Instance().Enqueue(InstantiateInThread<T>(k, prefab, allInfos[k]));
                        trustedObject.StartCoroutine(InstantiateInThread<T>(k, prefab, allInfos[k]));
                        //if(i++%10==0)
                        if (Time.realtimeSinceStartup - time > frame)
                        {
                            time = Time.realtimeSinceStartup;
                            yield return w;
                        }
                    }
                    else
                    {
                        if (allInstances.ContainsKey(TableName<T>() + k))
                        {
                            allInstances[TableName<T>() + k].gameObject.SetActive(false);
                        }
                    }
                }
            
            yield return null;


        }

        public static string TableName<T>(string typeName = null)
        {
            if (typeName != null)
                return "all" + typeName + "s";
            //  return "all" + obj.name + "s";
            return "all" + typeof(T).FullName + "s";
        }

        static IEnumerator InstantiateInThread<T>(string k, Transform prefab, T t)
        {
            // T t = allInfos[k];
            //Debug.Log(JsonConvert.SerializeObject(t));

            Transform prefabPivotInstance;
            if (!allInstances.ContainsKey(TableName<T>() + k))
            {
                prefabPivotInstance = Instantiate(prefab.gameObject).transform;
                prefabPivotInstance.name = k;
                prefabPivotInstance.transform.SetParent(prefab.transform.parent);
                prefabPivotInstance.transform.localScale = prefab.transform.parent.localScale;
                allInstances.Add(TableName<T>() + k, prefabPivotInstance);
                yield return null;

            }
            else
            {
                prefabPivotInstance = allInstances[TableName<T>() + k];
            }

            //here;
            EasyCrudsManager.SetTextAutomaticly<T>(prefabPivotInstance, t);
            yield return null;
        }
        public static bool ShouldShow<T>(object t)
        {
            // allPrefabs[TableName<T>()].GetComponent<>
            //Debug.Log("true = " + true);

            foreach (MonoBehaviour m in allPrefabs[TableName<T>(t.GetType().FullName)].GetComponents<MonoBehaviour>())
            {
                if (m.GetType() + "" == t.GetType().FullName + "Controller")
                {
                    //  Debug.Log(m.GetType() + " exists for real");
                    //if (m.GetType().GetField != null)
                    if (m.GetType().GetMethod("ShouldShow") != null)
                    {
                        // object[1] paramss = new object [1];

                        return (bool)m.GetType().GetMethod("ShouldShow").Invoke(m, new object[] { t });
                    }
                }
                else
                {
                    // Debug.Log(t.GetType().FullName + "Controller" + "does not exist");
                }
            }

            if (t.GetType().GetField("hide") != null)
            {
                if (t.GetType().GetField("hide").GetValue(t) + "" == "True") return false;
                return true;
            }


            return true;
        }
        public static Dictionary<string, T> LastMoves<T>(Dictionary<string, T> allInfos)
        {

            //allInfos = allInfos;
            foreach (MonoBehaviour m in allPrefabs[TableName<T>(typeof(T).FullName)].GetComponents<MonoBehaviour>())
            {
                if (m.GetType() + "" == typeof(T).FullName + "Controller")
                {

                    if (m.GetType().GetMethod("LastMoves") != null)
                    {
                        return (Dictionary<string, T>)m.GetType().GetMethod("LastMoves").Invoke(m, new object[] { allInfos });
                    }
                }
                else
                {

                    // Debug.Log(t.GetType().FullName + "Controller" + "does not exist");
                }
            }
            return allInfos;
        }

        public static string CleanName(string filename)
        {
           // return Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"[^0-9A-Za-z ,]", "").Replace(" ", "-").Replace("?", "").Replace("=", "");// Path.GetExtension(filename).Replace(" ", "-").Replace("?", "").Replace("=", "");
            return Regex.Replace(filename, @"[^0-9A-Za-z ,]", "").Replace(" ", "-").Replace("?", "").Replace("=", "");// Path.GetExtension(filename).Replace(" ", "-").Replace("?", "").Replace("=", "");
        }
        public static String LinkToFileLocalPath(string MediaUrl,string extension="")
        {
            filename = CleanName(MediaUrl);// Regex.Replace(MediaUrl, @"[^0-9A-Za-z ,]", "").Replace(" ", "-") + Path.GetExtension(MediaUrl);
            filePath = Path.Combine(Application.persistentDataPath, "brinis", filename);
            return filePath+extension;
        }
        public static string FileAlreadyExist(string MediaUrl,string extension="")
        {

            // filename = Path.GetFileName(MediaUrl);

            //Path.GetDirectoryName(LinkToFileLocalPath(MediaUrl));
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "brinis")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "brinis"));
            }

            if (File.Exists(LinkToFileLocalPath(MediaUrl,extension)))
            {
               // Debug.Log("fileExist");
                return LinkToFileLocalPath(MediaUrl,extension);
            }
            else
                return null;
        }
        public static string FinalLinkLocalOrAbsolute(string MediaUrl,string extension="")
        {
            if (!MediaUrl.ToLower().Contains("http"))
                if (!MediaUrl.ToLower().Contains("file://"))
                {
                    MediaUrl = "file://" + MediaUrl;
                }
            string link = MediaUrl;

            if (FileAlreadyExist(MediaUrl,extension) != null)
            {
                link = FileAlreadyExist(MediaUrl,extension);

            }
            return link;
        }
        public static IEnumerator DownloadVideo(VideoPlayer videoPlayer, string MediaUrl)
        {
            if(!string.IsNullOrWhiteSpace(MediaUrl))
            {
                string link = MediaUrl;
                DateTime lastModified = new DateTime();
                try
                {
                       // Debug.Log(" DownloadVideo "+MediaUrl);
                    link = FinalLinkLocalOrAbsolute(MediaUrl,".mp4");
                    float size = 0;
                    if (File.Exists(link))
                    {
                        FileInfo f = new FileInfo(link);
                        size = f.Length;
                        lastModified = f.LastWriteTimeUtc;
                        if(size<3000)
                        {
                            lastModified = System.DateTime.MinValue;
                        }
                        Debug.Log("local file size =" + size);
                        trustedObject.StartCoroutine(ShowVideoFromAbsoluteLink(videoPlayer, MediaUrl));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("brinis error " + e.Message);
                }
                //load video;
                
                //yield return DownloadVideoInnerRoutine(videoPlayer, MediaUrl, link);
                if (Application.internetReachability != NetworkReachability.NotReachable)
                    using (UnityWebRequest uwr = UnityWebRequest.Head(MediaUrl))
                {
                  
                        uwr.method = "HEAD";
                        yield return uwr.SendWebRequest();
                    try
                    {
                        Debug.Log(JsonConvert.SerializeObject(uwr.GetResponseHeaders()) + " at " + MediaUrl);
                        DateTime webLastModified = System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified").Replace("GMT", ""));
                        Debug.Log(lastModified - webLastModified +
                               "dates ----- " + lastModified + " absolute = " + webLastModified + " at " + MediaUrl);
                        if (lastModified < webLastModified)
                        {
                            Debug.Log(lastModified - System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified")) +
                                "dates are different " + lastModified + " absolute = " + System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified")) + " at " + MediaUrl);
                            link = MediaUrl;
                        }

                    }catch(Exception e)
                    {
                        Debug.Log("brinis error " + e.Message);
                    }
                }

                if (Application.internetReachability != NetworkReachability.NotReachable)
                if (link==MediaUrl)
                {
                    yield return DownloadVideoInnerRoutine(videoPlayer, MediaUrl, link);
                }
                //Debug.Log("link =" + link);
            }
        }
       static IEnumerator DownloadVideoInnerRoutine(VideoPlayer videoPlayer, string MediaUrl,string link)
        {
            WWW request = new WWW(link);
        
            yield return request;
      
            
                try
                {   
                    if (FileAlreadyExist(MediaUrl, ".mp4") == null || link == MediaUrl)
                    {
                    //byte[] bytes = ((Texture2D)rawImage.texture).EncodeToPNG();
                    //byte[] bytes = ((DownloadHandlerMovieTexture)request.downloadHandler).data;
                    byte[] bytes = (request.bytes);
                    File.WriteAllBytes(LinkToFileLocalPath(MediaUrl, ".mp4"), bytes);
                        Debug.Log("Writed file to " + LinkToFileLocalPath(MediaUrl, ".mp4"));
                    }
                }
            catch (Exception e)
            {
                Debug.LogError("brinis log " + e.Message);
            }
            //Debug.Log("video = " + LinkToFileLocalPath(MediaUrl, ".mp4"));
          trustedObject.StartCoroutine(ShowVideoFromAbsoluteLink(videoPlayer, MediaUrl));


        }
        public static IEnumerator ShowVideoFromAbsoluteLink(VideoPlayer videoPlayer, string MediaUrl)
        {
            videoPlayer.url = LinkToFileLocalPath(MediaUrl, ".mp4");
            if (videoPlayer.GetComponentInChildren<RawImage>(true))
            {
                    if (videoPlayer.targetTexture == null)
                {
                    //videoPlayer.targetMaterialRenderer.
                
                    videoPlayer.targetTexture = new RenderTexture((int)videoPlayer.GetComponentInChildren<RawImage>(true).rectTransform.rect.width,
                        (int)videoPlayer.GetComponentInChildren<RawImage>(true).rectTransform.rect.width, 16, RenderTextureFormat.ARGB32);
                    videoPlayer.targetTexture.Create();
                    videoPlayer.targetTexture.name = "brinis for " + Path.GetFileName(videoPlayer.url);
                }
            
                    videoPlayer.GetComponentInChildren<RawImage>(true).texture = videoPlayer.targetTexture;
                    videoPlayer.SetDirectAudioMute(0, true);
                    videoPlayer.Prepare();
                    videoPlayer.Stop();
                    videoPlayer.Play();
                    while (videoPlayer.frame < 5)
                    {
                        yield return null;
                    }
                    videoPlayer.SetDirectAudioMute(0, false);
                    videoPlayer.Stop();
            }
        }
       public static IEnumerator DownloadImage(RawImage rawImage, string MediaUrl)
        {
           
            string link = FinalLinkLocalOrAbsolute(MediaUrl, ".png");
            float size = 0;
            DateTime lastModified=new DateTime();
            if (File.Exists(link))
            {
                FileInfo f = new FileInfo(link);
                size = f.Length;
                lastModified = f.LastWriteTimeUtc;
                if (size < 3000)
                {
                    lastModified = System.DateTime.MinValue;
                }
                if (rawImage.texture==null)
                {
                    Texture2D texture= new Texture2D((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height, TextureFormat.ARGB32, false);
                    texture.name = "brinis for " +Path.GetFileName(link);
                    rawImage.texture = texture;
                }
                ((Texture2D)rawImage.mainTexture).LoadImage(File.ReadAllBytes(link));
                
                
            }
          
         //   yield return DonloadImageInnerRoutine(rawImage, MediaUrl, link);
            if(Application.internetReachability!= NetworkReachability.NotReachable)
            using (UnityWebRequest uwr = UnityWebRequest.Head(MediaUrl))
            {
                uwr.method = "HEAD";
                yield return uwr.SendWebRequest();
                Debug.Log(JsonConvert.SerializeObject(uwr.GetResponseHeaders()) + " at " + MediaUrl);
                DateTime webLastModified=new DateTime();
                if (uwr.GetResponseHeader("Last-Modified")!=null)
                webLastModified = System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified").Replace("GMT", ""));//, "ddd, dd MMM yyyy hh:mm:ss",new CultureInfo("fr-FR"));

                Debug.Log(lastModified - webLastModified +
                     "dates ----- " + lastModified + " absolute = " + webLastModified + " at " + MediaUrl);
                if (lastModified <webLastModified)
                    {
                    Debug.Log(lastModified - System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified")) +
                        "dates are different " + lastModified + " absolute = " + System.DateTime.Parse(uwr.GetResponseHeader("Last-Modified")) + " at " +MediaUrl);
                    link = MediaUrl;
                }
            }
            if (Application.internetReachability != NetworkReachability.NotReachable)
            if (link == MediaUrl)
            {
                yield return DonloadImageInnerRoutine(rawImage, MediaUrl, link);
            }
           // Debug.Log("link =" + link);
        }
           
          
        static IEnumerator DonloadImageInnerRoutine(RawImage rawImage, string MediaUrl, string link)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(link);
            request.timeout = 10;
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.LogError(request.error);
            else
            {
                try
                {
                    if (FileAlreadyExist(MediaUrl, ".png") == null || link==MediaUrl)
                    {
                        //byte[] bytes = ((Texture2D)rawImage.texture).EncodeToPNG();
                        byte[] bytes = ((DownloadHandlerTexture)request.downloadHandler).texture.EncodeToPNG();
                        File.WriteAllBytes(LinkToFileLocalPath(MediaUrl,".png"), bytes);
                        Debug.Log("Writed file to " + LinkToFileLocalPath(MediaUrl, ".png"));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("brinis log " + e.Message);
                }
                rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                rawImage.texture.name = "brinis for " + Path.GetFileName(link);
            }
        }
        public static void DownloadImageFunction(RawImage r,string link)
        {
            if (instance)
            {
                instance.StartCoroutine(DownloadImage(r, link));
            }
            else
            {
                FindObjectOfType<MonoBehaviour>().StartCoroutine(DownloadImage(r, link));
            }
        }
       
        public static IEnumerator DownloadSprite(SpriteRenderer r,string link)
        {
            WWW www = new WWW(link);
            yield return www;
            r.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
            yield break;
            if (instance)
            {
                yield return instance.StartCoroutine(DownloadImage(instance.pivotRawImageForSprites,link));
                // new Rect(0, 0, pivotRawImageForSprites.texture.width, pivotRawImageForSprites.texture.height)
                Texture2D texture=new Texture2D(instance.pivotRawImageForSprites.texture.width, instance.pivotRawImageForSprites.texture.height);
                texture.LoadImage(((Texture2D)instance.pivotRawImageForSprites.texture).EncodeToPNG());
                texture.Apply();
                r.sprite = Sprite.Create((Texture2D)texture, new Rect(0.0f, 0.0f,texture.width,
                texture.height), new Vector2(0.5f, 0.5f), 100.0f);
              
            }
        }
        public static void DownloadVideoFunction(VideoPlayer r, string link)
        {
            if (instance)
            {
                instance.StartCoroutine(DownloadVideo(r, link));
            }
            else
            {
                FindObjectOfType<MonoBehaviour>().StartCoroutine(DownloadVideo(r, link));
            }
        }
        static IEnumerator SetAutomaticlyRoutine<T>(Transform prefabPivotInstance, T t)
        {
            Transform pivot;
            FieldInfo[] allpropertys = typeof(T).GetFields();
            foreach (MonoBehaviour m in prefabPivotInstance.GetComponents<MonoBehaviour>())
            {
                if (m.GetType() + "" == t.GetType().FullName + "Controller")
                {
                    //  Debug.Log(m.GetType() + " exists for real");
                    //if (m.GetType().GetField != null)
                    if (m.GetType().GetField("info") != null)
                    {
                        m.GetType().GetField("info").SetValue(m, t);
                    }
                }
                else
                {
                    // Debug.Log(t.GetType().FullName + "Controller" + "does not exist");
                }
            }
            foreach (FieldInfo p in allpropertys)
            {
                //  Debug.Log(p.Name + "=" + p.GetValue(t));

                pivot = prefabPivotInstance.Find(p.Name);
                if (pivot)
                {

                    if (pivot.GetComponentInChildren<VideoPlayer>())
                    {
                        Debug.Log(" pivot.GetComponent<VideoPlayer>() ");
                        DownloadVideoFunction(pivot.GetComponent<VideoPlayer>(), p.GetValue(t) + "");
                    }
                    else
                    if (pivot.GetComponent<RawImage>())
                    {
                        DownloadImageFunction(pivot.GetComponent<RawImage>(), p.GetValue(t) + "");
                    }
                    if (pivot.GetComponent<SpriteRenderer>())
                    {
                        //  start(pivot.GetComponent<RawImage>(), p.GetValue(t) + "");
                        
                        if (instance && instance.pivotRawImageForSprites)
                        {
                           
                         yield return   trustedObject.StartCoroutine(DownloadSprite(pivot.GetComponent<SpriteRenderer>(), p.GetValue(t) + ""));
                        }
                        else
                        {
                            Debug.LogError("to show sprites you need to add to the scene EasyCrudsManager.cs and put the pivot raw image variable");
                        }

                    }

                    if (pivot.GetComponent<Text>())
                    {

                        pivot.GetComponent<Text>().text = "" + p.GetValue(t);
                        pivot.GetComponent<Text>().text = "" + p.GetValue(t);
                        pivot.GetComponent<Text>().text = "" + p.GetValue(t);
                    }
                    else
                    {
                        //Debug.Log("pivot.GetComponent<Text>() =false at " + p.Name);
                    }
                    if (pivot.GetComponent<InputField>())
                    {

                        pivot.GetComponent<InputField>().text = "" + p.GetValue(t);
                        pivot.GetComponent<InputField>().text = "" + p.GetValue(t);
                        pivot.GetComponent<InputField>().text = "" + p.GetValue(t);
                    }
                    if (pivot.GetComponent<Dropdown>())
                    {

                        if (p.FieldType.BaseType == typeof(Enum))
                        {

                            pivot.GetComponent<Dropdown>().ClearOptions();
                            pivot.GetComponent<Dropdown>().options = new List<Dropdown.OptionData>();

                            foreach (string r in Enum.GetNames(p.FieldType))
                            {
                                //Debug.Log(r);
                                pivot.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(r));
                            }
                        }
                        //  p.SetValue(t, pivot.GetComponent<Dropdown>().value);

                    }
                }
                else
                {
                    // Debug.LogError("didnt find text with name " + p.Name);
                }
                //Get  child with name and sset text in it ;
            }
            prefabPivotInstance.gameObject.SetActive(false);
            prefabPivotInstance.gameObject.SetActive(true);
        }
        public static void SetTextAutomaticly<T>(Transform prefabPivotInstance, T t)
        {
            trustedObject.StartCoroutine(SetAutomaticlyRoutine<T>(prefabPivotInstance, t));
        }

        public static T GetInfoAutomaticly<T>(Transform formularHead, System.Object t)
        {
             if (!SubscribeManager.Subscribed())
             {
                 Debug.LogError("you are not subscribed to the Listing Manager");
                 return (T)t;
             }
            FieldInfo[] allpropertys = t.GetType().GetFields();
           // Debug.Log(JsonConvert.SerializeObject(allpropertys));
            //  T t = new T();
            Transform pivot;
            foreach (FieldInfo p in allpropertys)
            {
                //  Debug.Log(p.Name + "=" + p.GetValue(t));

                pivot = formularHead.Find(p.Name);
                if (pivot)
                {

                    //  Debug.Log("  p.FieldType = " + p.FieldType);
                    Type type = p.FieldType;
                    if (pivot.GetComponent<Dropdown>())
                    {

                        if (p.FieldType.BaseType == typeof(Enum))
                        {

                            /*  foreach (typee r in Enum.GetValues(p.FieldType))
                            Debug.Log(r);*/
                            p.SetValue(t, pivot.GetComponent<Dropdown>().value);
                        }
                    }
                    if (pivot.GetComponent<InputField>())
                    {

                        if (p.FieldType.GetMethod("Parse", new Type[] { typeof(string) }) != null)
                        {
                            p.SetValue(t, p.FieldType.GetMethod("Parse", typesString).Invoke(p, new object[] { pivot.GetComponent<InputField>().text }));
                        }
                        if (p.FieldType == typeof(string))
                        {
                            p.SetValue(t, pivot.GetComponent<InputField>().text);
                        }
                    }
                    if (pivot.GetComponent<Text>())
                    {

                        if (p.FieldType.GetMethod("Parse", new Type[] { typeof(string) }) != null)
                        {
                            p.SetValue(t, p.FieldType.GetMethod("Parse", typesString).Invoke(p, new object[] { pivot.GetComponent<Text>().text }));
                        }
                        if (p.FieldType == typeof(string))
                        {
                            p.SetValue(t, pivot.GetComponent<Text>().text);
                        }
                    }
                }
            }
            return (T)t;
        }
        // Start is called before the first frame update

    }
}
//}

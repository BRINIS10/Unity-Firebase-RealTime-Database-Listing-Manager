using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoController : MonoBehaviour
{
    // Start is called before the first frame update
    public Info info=new Info();
    IEnumerator Start()
    {
        info.name = "Abderarhmen";
       info.video = "https://github.com/BRINIS10/StreamingVideos/blob/master/page_11.mp4";
        //info.video = "https://upload.wikimedia.org/wikipedia/commons/transcoded/9/99/Beautiful_Traditional_South_African_Dance%21.webm/Beautiful_Traditional_South_African_Dance%21.webm.360p.vp9.webm";
        info.num = 93;
        info.img= "https://pbs.twimg.com/profile_images/1289090172764618752/U4Li5gbN_400x400.jpg";
        yield return null;
        brinis.EasyCrudsManager.SetTextAutomaticly<Info>(transform,info);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class Info
{
    public string name;
    public int num;
    public string img;
   public string video;
    
}

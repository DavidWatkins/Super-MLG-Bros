using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;

public class AnimatedGifDrawer : MonoBehaviour
{
    public float speed = 0.05f;
    public Vector2 drawPosition;

    public static string gif_dir = "Gifs/";

    //Get them memes up in here
    public static string[] gifs = new string[] {
        "Dancing",
        "Dew",
        "Dew2",
        "Explosion",
        "Meatspin",
        "mhm",
        "rainbowfrog",
        "Time Of Death",
        "uwotm8",
        "snoop",
        "doritos",
        "illuminati",
        "illuminaughty",
        "edgy",
        "school",
        "winning"
    };

    private static bool isLoadedStatically = false;
    private static bool isGif;
    private int index, rot;

    private static List<Texture2D>[] gif_images;
    private List<Texture2D> gifFrames;

    void Awake()
    {

        if (!isLoadedStatically)
        {
            gif_images = new List<Texture2D>[gifs.Length];
            for (int i = 0; i < gifs.Length; i++)
            {
                List<Texture2D> tempFrames = new List<Texture2D>();
                Image gifImage = Image.FromFile(Application.dataPath + "/Resources/" + gif_dir + gifs[i] + ".gif");
                var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
                int frameCount = gifImage.GetFrameCount(dimension);
                for (int j = 0; j < frameCount; j++)
                {
                    gifImage.SelectActiveFrame(dimension, j);
                    var frame = new Bitmap(gifImage.Width, gifImage.Height);
                    System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
                    var frameTexture = new Texture2D(frame.Width, frame.Height);
                    for (int x = 0; x < frame.Width; x++)
                        for (int y = 0; y < frame.Height; y++)
                        {
                            System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                            frameTexture.SetPixel(x, frame.Height - 1 - y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped, and y?
                        } 
                    frameTexture.Apply();
                    tempFrames.Add(frameTexture);
                }


                gif_images[i] = tempFrames;
            }

            Debug.Log("Loaded Statically");
            isLoadedStatically = true;
        }
        System.Random r = new System.Random();
            isGif = true;
            gifFrames = gif_images[(new System.Random()).Next(gifs.Length)];        
        drawPosition = new Vector2(10000, 0);
    }

    void OnGUI()
    {
            GUI.DrawTexture(new Rect(drawPosition.x, drawPosition.y, gifFrames[0].width, gifFrames[0].height), gifFrames[(int)(Time.frameCount * speed) % gifFrames.Count]);
    }
}
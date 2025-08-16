using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace ExtremeRacing.Assets
{
    public class ProceduralAssetGenerator : MonoBehaviour
    {
        [Header("Texture Generation")]
        [SerializeField] private int _textureResolution = 512;
        [SerializeField] private bool _generateOnStart = true;

        [Header("Audio Generation")]
        [SerializeField] private int _audioSampleRate = 44100;
        [SerializeField] private float _audioDuration = 2f;

        public static ProceduralAssetGenerator Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (_generateOnStart)
            {
                StartCoroutine(GenerateAllAssets());
            }
        }

        private IEnumerator GenerateAllAssets()
        {
            Debug.Log("[ProceduralGenerator] Starting asset generation...");
            
            GenerateTextures();
            yield return new WaitForEndOfFrame();
            
            GenerateAudioClips();
            yield return new WaitForEndOfFrame();
            
            Debug.Log("[ProceduralGenerator] All assets generated!");
        }

        #region Texture Generation

        public void GenerateTextures()
        {
            // Vehicle textures
            CreateVehicleTextures();
            
            // Animal textures
            CreateAnimalTextures();
            
            // Environment textures
            CreateEnvironmentTextures();
            
            // UI textures
            CreateUITextures();
        }

        private void CreateVehicleTextures()
        {
            // Red Bull F1 livery
            CreateF1Livery("RedBull_F1_Livery", new Color(0.1f, 0.2f, 0.8f), Color.red, Color.yellow);
            
            // Ferrari red
            CreateMetallicTexture("Ferrari_Red", Color.red, 0.8f, 0.3f);
            
            // Lamborghini yellow
            CreateMetallicTexture("Lamborghini_Yellow", Color.yellow, 0.9f, 0.1f);
            
            // McLaren orange
            CreateMetallicTexture("McLaren_Orange", new Color(1f, 0.5f, 0f), 0.85f, 0.2f);
            
            // Carbon fiber texture
            CreateCarbonFiberTexture("Carbon_Fiber");
            
            // Rally car dirt/mud texture
            CreateDirtyTexture("Rally_Mud", new Color(0.6f, 0.4f, 0.2f));
            
            // Bike textures
            CreateBikeTexture("BMX_Graffiti", Color.yellow, true);
            CreateBikeTexture("Mountain_Bike_Clean", Color.green, false);
        }

        private void CreateAnimalTextures()
        {
            // Horse textures
            CreateAnimalFur("Horse_Brown", new Color(0.6f, 0.4f, 0.2f), 0.8f);
            CreateAnimalFur("Horse_Black", Color.black, 0.9f);
            CreateAnimalFur("Horse_White", Color.white, 0.7f);
            
            // Big cats
            CreateAnimalPattern("Cheetah_Spots", new Color(1f, 0.8f, 0.4f), Color.black, PatternType.Spots);
            CreateAnimalPattern("Lion_Mane", new Color(0.8f, 0.6f, 0.3f), new Color(0.6f, 0.4f, 0.2f), PatternType.Gradient);
            
            // Birds
            CreateFeatherTexture("Eagle_Feathers", new Color(0.4f, 0.3f, 0.1f), new Color(0.2f, 0.1f, 0.05f));
            CreateFeatherTexture("Falcon_Feathers", Color.gray, Color.white);
            
            // Elephant
            CreateAnimalSkin("Elephant_Skin", Color.gray, 0.3f);
            
            // Wolf
            CreateAnimalFur("Wolf_Gray", Color.gray, 0.9f);
        }

        private void CreateEnvironmentTextures()
        {
            // Track surfaces
            CreateAsphaltTexture("Track_Asphalt");
            CreateDirtTexture("Rally_Dirt");
            CreateSnowTexture("Mountain_Snow");
            CreateSandTexture("Desert_Sand");
            
            // Building textures
            CreateConcreteTexture("City_Concrete");
            CreateMetalTexture("Port_Metal");
            CreateGrassTexture("Environment_Grass");
            CreateRockTexture("Mountain_Rock");
        }

        private void CreateUITextures()
        {
            // Red Bull branding
            CreateRedBullLogo("RedBull_Logo");
            CreateGradientTexture("UI_Background", Color.black, new Color(0.1f, 0.1f, 0.2f));
            CreateGradientTexture("Button_Normal", new Color(0.2f, 0.6f, 1f), new Color(0.1f, 0.3f, 0.8f));
            CreateGradientTexture("Button_Hover", new Color(0.3f, 0.7f, 1f), new Color(0.2f, 0.4f, 0.9f));
        }

        #region Specific Texture Creators

        private Texture2D CreateF1Livery(string name, Color primaryColor, Color secondaryColor, Color accentColor)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = primaryColor;
                    
                    // Racing stripes
                    if (x % 64 < 16)
                    {
                        pixelColor = secondaryColor;
                    }
                    
                    // Sponsor areas
                    if (y > _textureResolution * 0.4f && y < _textureResolution * 0.6f)
                    {
                        if (x % 128 < 32)
                        {
                            pixelColor = accentColor;
                        }
                    }
                    
                    // Add metallic shine
                    float shine = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.2f;
                    pixelColor = Color.Lerp(pixelColor, Color.white, shine);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateMetallicTexture(string name, Color baseColor, float metallic, float roughness)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    // Base color
                    Color pixelColor = baseColor;
                    
                    // Metallic reflection
                    float reflection = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * metallic;
                    pixelColor = Color.Lerp(pixelColor, Color.white, reflection);
                    
                    // Roughness (surface imperfections)
                    float roughnessNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * roughness;
                    pixelColor = Color.Lerp(pixelColor, Color.gray, roughnessNoise * 0.3f);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateCarbonFiberTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    // Carbon fiber weave pattern
                    bool verticalFiber = (x / 8) % 2 == 0;
                    bool horizontalFiber = (y / 8) % 2 == 0;
                    
                    Color pixelColor;
                    if (verticalFiber && !horizontalFiber)
                    {
                        pixelColor = new Color(0.15f, 0.15f, 0.15f); // Dark fiber
                    }
                    else if (!verticalFiber && horizontalFiber)
                    {
                        pixelColor = new Color(0.25f, 0.25f, 0.25f); // Light fiber
                    }
                    else
                    {
                        pixelColor = new Color(0.1f, 0.1f, 0.1f); // Resin
                    }
                    
                    // Add subtle noise
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                    pixelColor += new Color(noise, noise, noise);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateDirtyTexture(string name, Color baseColor)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Dirt splatters
                    float dirt = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                    if (dirt > 0.6f)
                    {
                        pixelColor = Color.Lerp(pixelColor, new Color(0.3f, 0.2f, 0.1f), 0.7f);
                    }
                    
                    // Mud streaks
                    float streak = Mathf.PerlinNoise(x * 0.01f, y * 0.5f);
                    if (streak > 0.7f)
                    {
                        pixelColor = Color.Lerp(pixelColor, new Color(0.4f, 0.3f, 0.2f), 0.5f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateBikeTexture(string name, Color baseColor, bool hasGraffiti)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    if (hasGraffiti)
                    {
                        // Graffiti patterns
                        float graffiti = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                        if (graffiti > 0.7f)
                        {
                            // Random graffiti colors
                            float hue = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                            pixelColor = Color.HSVToRGB(hue, 0.8f, 0.9f);
                        }
                    }
                    else
                    {
                        // Clean bike with subtle wear
                        float wear = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                        if (wear > 0.8f)
                        {
                            pixelColor = Color.Lerp(pixelColor, Color.gray, 0.3f);
                        }
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private enum PatternType { Spots, Stripes, Gradient }

        private Texture2D CreateAnimalPattern(string name, Color baseColor, Color patternColor, PatternType pattern)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    switch (pattern)
                    {
                        case PatternType.Spots:
                            float spotNoise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                            if (spotNoise > 0.6f)
                            {
                                float spotSize = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                                if (spotSize > 0.5f)
                                {
                                    pixelColor = patternColor;
                                }
                            }
                            break;
                            
                        case PatternType.Stripes:
                            if (Mathf.Sin(y * 0.1f) > 0f)
                            {
                                pixelColor = patternColor;
                            }
                            break;
                            
                        case PatternType.Gradient:
                            float gradientFactor = (float)y / _textureResolution;
                            pixelColor = Color.Lerp(baseColor, patternColor, gradientFactor);
                            break;
                    }
                    
                    // Add fur texture
                    float fur = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.1f;
                    pixelColor += new Color(fur, fur, fur);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateAnimalFur(string name, Color baseColor, float furDensity)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Fur texture
                    float fur1 = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * furDensity;
                    float fur2 = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * (1f - furDensity);
                    
                    float furValue = (fur1 + fur2) * 0.3f;
                    pixelColor = Color.Lerp(pixelColor, Color.white, furValue);
                    
                    // Hair direction
                    float direction = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    if (direction > 0.7f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.black, 0.1f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateFeatherTexture(string name, Color primaryColor, Color secondaryColor)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    // Feather shaft pattern
                    float distanceFromCenter = Mathf.Abs(x - _textureResolution / 2) / (float)_textureResolution;
                    
                    Color pixelColor;
                    if (distanceFromCenter < 0.05f)
                    {
                        // Shaft
                        pixelColor = Color.black;
                    }
                    else
                    {
                        // Barbs
                        float featherPattern = Mathf.Sin(x * 0.2f) * Mathf.Sin(y * 0.1f);
                        pixelColor = featherPattern > 0 ? primaryColor : secondaryColor;
                        
                        // Add iridescence
                        float iridescence = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.3f;
                        pixelColor = Color.Lerp(pixelColor, Color.blue, iridescence);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateAnimalSkin(string name, Color baseColor, float roughness)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Skin wrinkles
                    float wrinkle1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float wrinkle2 = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    
                    float wrinkleValue = (wrinkle1 + wrinkle2 * 0.5f) * roughness;
                    
                    if (wrinkleValue > 0.6f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.black, 0.3f);
                    }
                    else if (wrinkleValue < 0.3f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.2f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateAsphaltTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(0.2f, 0.2f, 0.2f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Asphalt grain
                    float grain = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.3f;
                    pixelColor += new Color(grain, grain, grain);
                    
                    // Tire marks
                    if (UnityEngine.Random.Range(0f, 1f) < 0.05f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.black, 0.5f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateDirtTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(0.6f, 0.4f, 0.2f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Dirt variation
                    float variation = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.4f;
                    pixelColor += new Color(variation, variation * 0.8f, variation * 0.6f);
                    
                    // Rocks
                    if (Mathf.PerlinNoise(x * 0.05f, y * 0.05f) > 0.8f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.gray, 0.6f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateSnowTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    // Snow is mostly white with subtle variations
                    float snowNoise = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.1f;
                    Color pixelColor = Color.white - new Color(snowNoise, snowNoise, snowNoise * 0.5f);
                    
                    // Ice patches
                    if (Mathf.PerlinNoise(x * 0.03f, y * 0.03f) > 0.7f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.cyan, 0.3f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateSandTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(1f, 0.9f, 0.6f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Sand grains
                    float grain1 = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.2f;
                    float grain2 = Mathf.PerlinNoise(x * 0.5f, y * 0.5f) * 0.1f;
                    
                    pixelColor += new Color(grain1 + grain2, (grain1 + grain2) * 0.8f, (grain1 + grain2) * 0.6f);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateConcreteTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(0.7f, 0.7f, 0.7f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Concrete texture
                    float concrete = Mathf.PerlinNoise(x * 0.03f, y * 0.03f) * 0.3f;
                    pixelColor += new Color(concrete, concrete, concrete);
                    
                    // Cracks
                    if (Mathf.PerlinNoise(x * 0.01f, y * 0.8f) > 0.9f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.black, 0.8f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateMetalTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(0.6f, 0.6f, 0.7f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Metal scratches
                    if (y % 4 == 0)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.3f);
                    }
                    
                    // Rust spots
                    if (Mathf.PerlinNoise(x * 0.05f, y * 0.05f) > 0.8f)
                    {
                        pixelColor = Color.Lerp(pixelColor, new Color(0.8f, 0.4f, 0.2f), 0.6f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateGrassTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = Color.green;
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Grass variation
                    float grass = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.5f;
                    pixelColor = Color.Lerp(pixelColor, new Color(0.2f, 0.8f, 0.2f), grass);
                    
                    // Dirt patches
                    if (Mathf.PerlinNoise(x * 0.02f, y * 0.02f) > 0.9f)
                    {
                        pixelColor = Color.Lerp(pixelColor, new Color(0.5f, 0.3f, 0.1f), 0.7f);
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateRockTexture(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            Color baseColor = new Color(0.5f, 0.5f, 0.6f);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    Color pixelColor = baseColor;
                    
                    // Rock texture
                    float rock1 = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.4f;
                    float rock2 = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.2f;
                    
                    pixelColor += new Color(rock1 + rock2, rock1 + rock2, (rock1 + rock2) * 1.1f);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateRedBullLogo(string name)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            int centerX = _textureResolution / 2;
            int centerY = _textureResolution / 2;
            int radius = _textureResolution / 3;
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    
                    Color pixelColor = Color.clear;
                    
                    // Red Bull circle
                    if (distance <= radius)
                    {
                        if (distance >= radius - 20)
                        {
                            pixelColor = Color.red; // Border
                        }
                        else
                        {
                            pixelColor = new Color(0.1f, 0.2f, 0.8f); // Blue background
                        }
                        
                        // Bulls (simplified)
                        if (distance < radius / 2)
                        {
                            float angle = Mathf.Atan2(y - centerY, x - centerX) * Mathf.Rad2Deg;
                            if (Mathf.Abs(angle) < 30f || Mathf.Abs(Mathf.Abs(angle) - 180f) < 30f)
                            {
                                pixelColor = Color.yellow;
                            }
                        }
                    }
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        private Texture2D CreateGradientTexture(string name, Color topColor, Color bottomColor)
        {
            Texture2D texture = new Texture2D(_textureResolution, _textureResolution);
            
            for (int x = 0; x < _textureResolution; x++)
            {
                for (int y = 0; y < _textureResolution; y++)
                {
                    float t = (float)y / _textureResolution;
                    Color pixelColor = Color.Lerp(bottomColor, topColor, t);
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            texture.Apply();
            SaveTexture(texture, name);
            return texture;
        }

        #endregion

        #endregion

        #region Audio Generation

        public void GenerateAudioClips()
        {
            // Engine sounds
            CreateEngineSound("Engine_F1", 800f, 8000f, true);
            CreateEngineSound("Engine_Supercar", 400f, 6000f, true);
            CreateEngineSound("Engine_Rally", 300f, 5000f, false);
            CreateEngineSound("Engine_Motorcycle", 200f, 7000f, true);
            CreateEngineSound("Engine_Bike", 0f, 0f, false); // Silent for bikes
            
            // Sound effects
            CreateBrakeSound("Brake_Screech");
            CreateJumpSound("Jump_Whoosh");
            CreateCrashSound("Crash_Impact");
            CreatePickupSound("Pickup_Coin");
            
            // Music
            CreateRaceMusic("Race_Electronic");
            CreateMenuMusic("Menu_Ambient");
        }

        private AudioClip CreateEngineSound(string name, float baseFreq, float maxFreq, bool hasEngine)
        {
            if (!hasEngine)
            {
                return CreateSilentClip(name);
            }
            
            int samples = Mathf.RoundToInt(_audioSampleRate * _audioDuration);
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Engine RPM simulation (cycling)
                float rpm = baseFreq + (maxFreq - baseFreq) * (0.5f + 0.5f * Mathf.Sin(time * 2f));
                
                // Engine harmonics
                float fundamental = Mathf.Sin(2f * Mathf.PI * rpm * time);
                float harmonic2 = 0.5f * Mathf.Sin(2f * Mathf.PI * rpm * 2f * time);
                float harmonic3 = 0.3f * Mathf.Sin(2f * Mathf.PI * rpm * 3f * time);
                
                // Engine noise
                float noise = (UnityEngine.Random.value - 0.5f) * 0.1f;
                
                audioData[i] = (fundamental + harmonic2 + harmonic3 + noise) * 0.3f;
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateBrakeSound(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 1f); // 1 second
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // High frequency screech with decay
                float frequency = 2000f + 1000f * Mathf.Exp(-time * 3f);
                float amplitude = Mathf.Exp(-time * 2f);
                
                audioData[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * amplitude * 0.5f;
                
                // Add noise
                audioData[i] += (UnityEngine.Random.value - 0.5f) * 0.2f * amplitude;
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateJumpSound(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 0.5f); // 0.5 seconds
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Whoosh sound - decreasing frequency
                float frequency = 500f * (1f - time * 2f);
                float amplitude = 1f - time * 2f;
                
                if (amplitude > 0)
                {
                    audioData[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * amplitude * 0.3f;
                    
                    // Wind noise
                    audioData[i] += (UnityEngine.Random.value - 0.5f) * 0.4f * amplitude;
                }
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateCrashSound(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 2f); // 2 seconds
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Initial impact
                if (time < 0.1f)
                {
                    audioData[i] = (UnityEngine.Random.value - 0.5f) * 2f * (1f - time * 10f);
                }
                // Debris falling
                else if (time < 1.5f)
                {
                    float debris = 0f;
                    if (UnityEngine.Random.value < 0.1f) // Random debris sounds
                    {
                        debris = (UnityEngine.Random.value - 0.5f) * 0.5f;
                    }
                    audioData[i] = debris * Mathf.Exp(-(time - 0.1f));
                }
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreatePickupSound(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 0.3f); // 0.3 seconds
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Coin pickup sound - ascending frequencies
                float freq1 = 800f * (1f + time * 2f);
                float freq2 = 1200f * (1f + time * 1.5f);
                
                float amplitude = 1f - time * 3.33f; // Decay over 0.3 seconds
                
                if (amplitude > 0)
                {
                    audioData[i] = (Mathf.Sin(2f * Mathf.PI * freq1 * time) + 
                                   0.5f * Mathf.Sin(2f * Mathf.PI * freq2 * time)) * amplitude * 0.4f;
                }
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateRaceMusic(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 30f); // 30 seconds loop
            float[] audioData = new float[samples];
            
            float bpm = 140f;
            float beatDuration = 60f / bpm;
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Electronic beat
                float beat = time % beatDuration;
                float kick = 0f;
                if (beat < 0.1f)
                {
                    kick = Mathf.Sin(2f * Mathf.PI * 60f * time) * (1f - beat * 10f);
                }
                
                // Bass line
                float bassFreq = 80f + 20f * Mathf.Sin(time * 0.5f);
                float bass = 0.3f * Mathf.Sin(2f * Mathf.PI * bassFreq * time);
                
                // Lead synth
                float leadFreq = 440f + 110f * Mathf.Sin(time * 0.25f);
                float lead = 0.2f * Mathf.Sin(2f * Mathf.PI * leadFreq * time);
                
                audioData[i] = kick + bass + lead;
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateMenuMusic(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 60f); // 60 seconds loop
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / _audioSampleRate;
                
                // Ambient pad
                float pad1 = 0.1f * Mathf.Sin(2f * Mathf.PI * 220f * time);
                float pad2 = 0.08f * Mathf.Sin(2f * Mathf.PI * 330f * time);
                float pad3 = 0.06f * Mathf.Sin(2f * Mathf.PI * 440f * time);
                
                // Slow melody
                float melodyFreq = 440f * Mathf.Pow(2f, Mathf.Sin(time * 0.1f) * 0.5f);
                float melody = 0.15f * Mathf.Sin(2f * Mathf.PI * melodyFreq * time) * 
                              (0.5f + 0.5f * Mathf.Sin(time * 2f));
                
                audioData[i] = pad1 + pad2 + pad3 + melody;
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private AudioClip CreateSilentClip(string name)
        {
            int samples = Mathf.RoundToInt(_audioSampleRate * 0.1f); // 0.1 seconds
            float[] audioData = new float[samples]; // All zeros = silence
            
            AudioClip clip = AudioClip.Create(name, samples, 1, _audioSampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        #endregion

        #region Utility Methods

        private void SaveTexture(Texture2D texture, string name)
        {
            // In a real implementation, save to AssetManager or Resources
            Debug.Log($"[ProceduralGenerator] Generated texture: {name}");
            
            // Apply to AssetManager if available
            if (AssetManager.Instance != null)
            {
                AssetManager.Instance.SaveUserTexture(texture, name);
            }
        }

        private void SaveAudioClip(AudioClip clip, string name)
        {
            Debug.Log($"[ProceduralGenerator] Generated audio: {name}");
            // In a real implementation, save to AssetManager or Resources
        }

        #endregion

        // Public API
        public Texture2D GenerateCustomVehicleTexture(Color primaryColor, Color secondaryColor, string pattern = "racing")
        {
            switch (pattern.ToLower())
            {
                case "racing":
                    return CreateF1Livery($"Custom_Racing_{primaryColor}_{secondaryColor}", primaryColor, secondaryColor, Color.white);
                case "metallic":
                    return CreateMetallicTexture($"Custom_Metallic_{primaryColor}", primaryColor, 0.8f, 0.2f);
                case "dirty":
                    return CreateDirtyTexture($"Custom_Dirty_{primaryColor}", primaryColor);
                default:
                    return CreateMetallicTexture($"Custom_Default_{primaryColor}", primaryColor, 0.5f, 0.5f);
            }
        }

        public Texture2D GenerateCustomAnimalTexture(Color baseColor, string species = "generic")
        {
            switch (species.ToLower())
            {
                case "horse":
                    return CreateAnimalFur($"Custom_Horse_{baseColor}", baseColor, 0.8f);
                case "cat":
                    return CreateAnimalPattern($"Custom_Cat_{baseColor}", baseColor, Color.black, PatternType.Spots);
                case "bird":
                    return CreateFeatherTexture($"Custom_Bird_{baseColor}", baseColor, Color.white);
                default:
                    return CreateAnimalFur($"Custom_Animal_{baseColor}", baseColor, 0.7f);
            }
        }
    }
}
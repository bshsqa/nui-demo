using System.Runtime.InteropServices;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace NUISample
{
    class ClippingMaskView : View
    {
        static readonly string VERTEX_SHADER =
        "attribute mediump vec2 aPosition;\n" +
        "uniform mediump mat4 uMvpMatrix;\n" +
        "uniform mediump vec3 uSize;\n" +
        "attribute mediump vec2 aTexCoord;\n" +
        "varying mediump vec2 vTexCoord;\n" +
        "void main()\n" +
        "{\n" +
        "    vec4 position = vec4(aPosition,0.0,1.0)*vec4(uSize,1.0);\n" +
        "    gl_Position = uMvpMatrix * position;\n" +
        "    vTexCoord = aPosition + vec2( 0.5 );\n" +
        "}\n";



        static readonly string FRAGMENT_SHADER =
        "uniform lowp vec4 uColor;\n" +
        // Clipped image texture to be masked
        "uniform sampler2D sTexture;\n" +
        // Mask Texture
        "uniform sampler2D sMaskTexture;\n" +
        "varying mediump vec2 vTexCoord;\n" +
        "void main()\n" +
        "{\n" +
        "    gl_FragColor = texture2D( sTexture, vTexCoord ) * uColor;\n" +
             // Apply alpha masking
        "    gl_FragColor.a *= texture2D( sMaskTexture, vTexCoord ).a;\n" +
        "}\n";
        

        /// <summary>
        /// View which is clipping image and applying mask
        /// </summary>
        /// <param name="resourceImageUrl">Image which will be cripped</param>
        /// <param name="maskImageUrl">Image for masking</param>
        public ClippingMaskView(string resourceImageUrl, string maskImageUrl)
        {
            // Load mask image file and make PixelData
            PixelData pixelData = PixelBuffer.Convert(
                ImageLoading.LoadImageFromFile( 
                    maskImageUrl,
                    new Size2D(),
                    FittingModeType.ScaleToFill
                )
            );

            // Make mask image texture and upload.
            Texture maskTexture = new Texture(
                TextureType.TEXTURE_2D,
                pixelData.GetPixelFormat(),
                pixelData.GetWidth(),
                pixelData.GetHeight()
            );
            maskTexture.Upload(pixelData);

            Size maskImageSize = new Size(maskTexture.GetWidth(), maskTexture.GetHeight());

            // Background Image will be clipped
            BackgroundImage = new ImageView()
            {
                PositionUsesPivotPoint = true,
                PivotPoint = Tizen.NUI.PivotPoint.Center,
                ParentOrigin = Tizen.NUI.ParentOrigin.Center,
                Size = maskImageSize,
                ResourceUrl = resourceImageUrl,
            };
            Add(BackgroundImage);

            // Set properties for render task
            Camera camera = new Camera(new Vector2(maskImageSize.Width,maskImageSize.Height))
            {
                PositionUsesPivotPoint = true,
                PivotPoint = Tizen.NUI.PivotPoint.Center,
                ParentOrigin = Tizen.NUI.ParentOrigin.Center,
            };
            camera.SetInvertYAxis(true);
            Add(camera);

            RenderTask task = Window.Instance.GetRenderTaskList().CreateTask();
            task.SetRefreshRate((uint)RenderTask.RefreshRate.REFRESH_ALWAYS);
            task.SetSourceView(BackgroundImage);
            task.SetExclusive(true);
            task.SetInputEnabled(false);
            task.SetClearColor(new Vector4(1.0f,1.0f,1.0f,1.0f));
            task.SetClearEnabled(true);
            task.SetCamera(camera);

            // Clipped Texture
            Texture clippedTexture = new Texture(
                TextureType.TEXTURE_2D,
                PixelFormat.RGBA8888,
                (uint)maskImageSize.Width,
                (uint)maskImageSize.Height
            );

            FrameBuffer frameBuffer = new FrameBuffer(
                (uint)maskImageSize.Width,
                (uint)maskImageSize.Height,
                (uint)FrameBuffer.Attachment.Mask.NONE
            );

            frameBuffer.AttachColorTexture( clippedTexture );
            task.SetFrameBuffer(frameBuffer);

            /* Create Renderer to apply mask */
            const float halfWidth = 0.5f;
            const float halfHeight = 0.5f;
            Vector2[] quadVertexData = new Vector2[4] { { Vector2(-halfWidth, -halfHeight) },
                                                        { Vector2(-halfWidth, halfHeight)  },
                                                        { Vector2( halfWidth, -halfHeight) },
                                                        { Vector2( halfWidth, halfHeight)  } };
            PropertyMap vertexFormat = new PropertyMap();
            vertexFormat.Add("aPosition", new PropertyValue((int)PropertyType.Vector2));
            PropertyBuffer vertexBuffer = new PropertyBuffer(vertexFormat);
            vertexBuffer.SetData(quadVertexData, 4);

            /* Create geometry */
            Geometry geometry = new Geometry();
            geometry.AddVertexBuffer(vertexBuffer);
            geometry.SetType(Geometry.Type.TRIANGLE_STRIP);

            /* Create Shader */
            Shader shader = new Shader(VERTEX_SHADER, FRAGMENT_SHADER);

            TextureSet textureSet = new TextureSet();
            textureSet.SetTexture(0u, clippedTexture);
            textureSet.SetTexture(1u, maskTexture);

            Renderer renderer = new Renderer(geometry,shader);
            renderer.SetTextures(textureSet);

            AddRenderer(renderer);
        }

        public ImageView BackgroundImage{get;set;}
    }
}
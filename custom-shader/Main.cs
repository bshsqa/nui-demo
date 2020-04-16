/*
 * Copyright (c) 2019 Samsung Electronics Co., Ltd.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace NUISample
{
    class MainApplication : NUIApplication
    {
        private PanGestureDetector detector;
        private View container;
        private ShaderEffectView shaderEffectView;
        /// <summary>
        /// Override to create the required scene
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            Window window = Window.Instance;
            window.BackgroundColor = Color.White;

            View root = new View()
            {
                Size = new Size(Window.Instance.WindowSize),
                Layout = new LinearLayout()
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical
                },
            };
            window.Add(root);

            container = new View()
            {
                Weight = 0,
                Layout = new AbsoluteLayout(),
                WidthSpecification = Window.Instance.WindowSize.Width,
                HeightSpecification = Window.Instance.WindowSize.Width,
                ClippingMode = ClippingModeType.ClipToBoundingBox,
            };
            root.Add(container);

            ImageView backgroundImage = new ImageView()
            {
                WidthSpecification = LayoutParamPolicies.MatchParent,
                HeightSpecification = LayoutParamPolicies.MatchParent,
                ResourceUrl = "./res/background_image.jpg",
                Opacity = 0.1f,
            };
            container.Add(backgroundImage);

            shaderEffectView = new ShaderEffectView("./res/background_image.jpg", "./res/mask_image.png")
            {
                Size = new Size(150, 150),
                PositionUsesPivotPoint = true,
                Position = new Position(container.Size.Width / 2.0f, container.Size.Height / 2.0f),
            };
            container.Add(shaderEffectView);

            float positionX = (shaderEffectView.Position.X - shaderEffectView.Size.Width / 2.0f) / container.Size.Width;
            float positionY = (shaderEffectView.Position.Y - shaderEffectView.Size.Height / 2.0f) / container.Size.Height;
            float sizeWidth = shaderEffectView.Size.Width / container.Size.Width;
            float sizeHeight = shaderEffectView.Size.Height / container.Size.Height;

            shaderEffectView.MaskImage.PixelArea = new Vector4(positionX, positionY, sizeWidth, sizeHeight);

            detector = new PanGestureDetector();
            detector.Attach(container);
            detector.Detected += (object source, PanGestureDetector.DetectedEventArgs args) =>
            {
                shaderEffectView.Position = new Position(args.PanGesture.Position);
                positionX = (shaderEffectView.Position.X - shaderEffectView.Size.Width / 2.0f) / container.Size.Width;
                positionY = (shaderEffectView.Position.Y - shaderEffectView.Size.Height / 2.0f) / container.Size.Height;
                sizeWidth = shaderEffectView.Size.Width / container.Size.Width;
                sizeHeight = shaderEffectView.Size.Height / container.Size.Height;

                shaderEffectView.MaskImage.PixelArea = new Vector4(positionX, positionY, sizeWidth, sizeHeight);
            };

            View controlPannel = new View()
            {
                WidthSpecification = Window.Instance.WindowSize.Width,
                HeightSpecification = Window.Instance.WindowSize.Height - Window.Instance.WindowSize.Width,
                Layout = new AbsoluteLayout(),
                BackgroundColor = new Color("#0A0A0A"),
            };

            foreach (PannelButton.Direction type in Enum.GetValues(typeof(PannelButton.Direction)))
            {
                PannelButton button = new PannelButton()
                {
                    Size = new Size(60, 60),
                    CornerRadius = 30,
                    Type = type,
                };

                button.Style.BackgroundColor = new ColorSelector()
                {
                    Pressed = new Color("#A3A3A3"),
                    Normal = Color.White,
                };

                button.ClickEvent += (object source, Button.ClickEventArgs args) =>
                {
                    Position newPosition = CalculatePositionByDirection(type);

                    positionX = (newPosition.X - shaderEffectView.Size.Width / 2.0f) / container.Size.Width;
                    positionY = (newPosition.Y - shaderEffectView.Size.Height / 2.0f) / container.Size.Height;
                    sizeWidth = shaderEffectView.Size.Width / container.Size.Width;
                    sizeHeight = shaderEffectView.Size.Height / container.Size.Height;

                    Animation moveAnimation = new Animation(150);
                    moveAnimation.AnimateTo(shaderEffectView, "position", newPosition);
                    moveAnimation.AnimateTo(shaderEffectView.MaskImage, "pixelArea", new Vector4(positionX, positionY, sizeWidth, sizeHeight));
                    moveAnimation.Play();
                };

                controlPannel.Add(button);
            }

            root.Add(controlPannel);

            // Respond to key events
            window.KeyEvent += OnKeyEvent;
        }

        private Position CalculatePositionByDirection(PannelButton.Direction direction)
        {
            Position position = new Position();
            Size effectViewSize = shaderEffectView.Size / 2.0f;
            Size containerSize = container.Size;

            switch (direction)
            {
                case PannelButton.Direction.TopLeft:
                    {
                        position = new Position(effectViewSize.Width, effectViewSize.Height);
                        break;
                    }
                case PannelButton.Direction.TopRight:
                    {
                        position = new Position(containerSize.Width - effectViewSize.Width, effectViewSize.Height);
                        break;
                    }
                case PannelButton.Direction.Center:
                    {
                        position = new Position(containerSize.Width / 2.0f, containerSize.Height / 2.0f);
                        break;
                    }
                case PannelButton.Direction.BottomLeft:
                    {
                        position = new Position(effectViewSize.Width, containerSize.Height - effectViewSize.Height);
                        break;
                    }
                case PannelButton.Direction.BottomRight:
                    {
                        position = new Position(containerSize.Width - effectViewSize.Width, containerSize.Height - effectViewSize.Height);
                        break;
                    }
            }

            return position;
        }

        /// <summary>
        /// Called when any key event is received.
        /// Will use this to exit the application if the Back or Escape key is pressed
        /// </summary>
        private void OnKeyEvent(object sender, Window.KeyEventArgs eventArgs)
        {
            if (eventArgs.Key.State == Key.StateType.Down)
            {
                switch (eventArgs.Key.KeyPressedName)
                {
                    case "Escape":
                    case "Back":
                        {
                            Exit();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Forces app to use one thread to access NUI
        static void Main(string[] args)
        {
            MainApplication example = new MainApplication();
            example.Run(args);
        }
    }
}
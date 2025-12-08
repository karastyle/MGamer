// <copyright file="InfluenceCurveGraphView.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System;
using System.Globalization;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    [UxmlElement]
    public partial class InfluenceCurveGraphView : BaseView
    {
        public InfluenceCurveGraphView() : base(UIBuilderResourcePaths.InfluenceCurveGraphView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            InitInfluenceCurveView();
            InitInputView();
            InitInputLabel();
            InitScoreLabel();
        }

        #region Input/Score Fields

        private float input;

        [CreateProperty]
        public float Input
        {
            get => input;
            set
            {
                if (Math.Abs(input - value) < MathUtils.Epsilon)
                    return;

                input = value;
                OnInputChanged();
            }
        }

        private Label inputLabel;

        private Label scoreLabel;

        #endregion

        #region InfluenceCurve Fields

        private InfluenceCurve influenceCurve;

        [CreateProperty]
        public InfluenceCurve InfluenceCurve
        {
            get => influenceCurve;
            set
            {
                if (influenceCurve.Equals(value))
                    return;
                influenceCurve = value;

                OnInfluenceCurveChanged();
            }
        }

        #endregion

        #region InfluenceCurveView Fields

        private readonly Vector2 min = new(0f, 0f);
        private readonly Vector2 max = new(1f, 1f);

        private readonly Color gridColor = new(0.5f, 0.5f, 0.5f);
        private readonly Color curveViewBackgroundColor = Color.black;
        private readonly Color curveColor = Color.green;
        private readonly float gridIncrements = 0.1f;

        // private float width = 100f;
        // private float height = 100f;

        private readonly float gridLineWidth = 0.5f;
        private readonly float curveLineWidth = 2f;

        private readonly float inputCircleRadius = 5f;

        private VisualElement influenceCurveView;

        #endregion

        #region InputView Fields

        private readonly Color inputViewBackgroundColor = new(0.0f, 0.0f, 0.0f, 0.0f);
        private VisualElement inputView;

        #endregion

        #region Event Functions

        private void OnInputChanged()
        {
            RepaintInputView();
            UpdateInputLabel();
            UpdateScoreLabel();
        }

        private void OnInfluenceCurveChanged()
        {
            RepaintInfluenceCurveView();
            RepaintInputView();
            UpdateInputLabel();
            UpdateScoreLabel();
        }

        #endregion

        #region Input/Score Functions

        private void InitScoreLabel()
        {
            scoreLabel = this.Q<Label>("ScoreValueLabel");
            if (scoreLabel == null) return;

            UpdateScoreLabel();
        }

        private void InitInputLabel()
        {
            inputLabel = this.Q<Label>("InputValueLabel");
            if (inputLabel == null) return;

            UpdateInputLabel();
        }

        private void UpdateInputLabel()
        {
            inputLabel.text = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", input);
        }

        private void UpdateScoreLabel()
        {
            float score = CalculateScore(input);
            scoreLabel.text = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", score);
        }

        private float CalculateScore(float input)
        {
            float score = InfluenceCurveUtils.Evaluate(input, in influenceCurve);
            // Debug.Log($"GetCurrentScore input: {InputValue} score: {score}");
            return score;
        }

        #endregion

        #region InputView

        private void InitInputView()
        {
            inputView = this.Q<VisualElement>("InputView");
            if (inputView == null) return;

            inputView.generateVisualContent += DrawInputView;
            inputView.style.backgroundColor = inputViewBackgroundColor;
        }

        private void RepaintInputView()
        {
            inputView.MarkDirtyRepaint();
        }

        private void DrawInputView(MeshGenerationContext context)
        {
            Painter2D paint2D = context.painter2D;
            float score = CalculateScore(input);
            Vector2 graphCoord = new(input, score);
            if (GetPixelCoordOfGraphCoord(graphCoord, out Vector2 pixelCoord))
            {
                paint2D.fillColor = curveColor;
                paint2D.BeginPath();
                paint2D.Arc(pixelCoord, inputCircleRadius, 0, Angle.Degrees(360));
                paint2D.Fill();
            }
        }

        #endregion

        #region InfluenceCurveView

        private void InitInfluenceCurveView()
        {
            influenceCurveView = this.Q<VisualElement>("InfluenceCurveView");
            if (influenceCurveView == null) return;

            influenceCurveView.generateVisualContent += DrawInfluenceCurveView;
            influenceCurveView.style.backgroundColor = curveViewBackgroundColor;
        }

        private void RepaintInfluenceCurveView()
        {
            influenceCurveView.MarkDirtyRepaint();
        }

        private void DrawInfluenceCurveView(MeshGenerationContext context)
        {
            Painter2D paint2D = context.painter2D;
            paint2D.lineJoin = LineJoin.Round;
            paint2D.lineCap = LineCap.Round;

            // Grid
            {
                paint2D.strokeColor = gridColor;
                paint2D.lineWidth = gridLineWidth;

                // Horizontal
                float gridCounter = Mathf.Floor(min.y * (1f / gridIncrements)) * gridIncrements;
                while (gridCounter < max.y)
                {
                    if (GetPixelCoordOfGraphCoord(new Vector2(min.x, gridCounter), out Vector2 x0) &&
                        GetPixelCoordOfGraphCoord(new Vector2(max.x, gridCounter), out Vector2 x1))
                    {
                        paint2D.BeginPath();
                        paint2D.MoveTo(x0);
                        paint2D.LineTo(x1);
                        paint2D.Stroke();
                    }

                    gridCounter += gridIncrements;
                }

                // Vertical
                gridCounter = Mathf.Floor(min.x * (1f / gridIncrements)) * gridIncrements;
                while (gridCounter < max.x)
                {
                    if (GetPixelCoordOfGraphCoord(new Vector2(gridCounter, min.y), out Vector2 y0) &&
                        GetPixelCoordOfGraphCoord(new Vector2(gridCounter, max.y), out Vector2 y1))
                    {
                        paint2D.BeginPath();
                        paint2D.MoveTo(y0);
                        paint2D.LineTo(y1);
                        paint2D.Stroke();
                    }

                    gridCounter += gridIncrements;
                }
            }

            // Curve
            {
                paint2D.strokeColor = curveColor;
                paint2D.lineWidth = curveLineWidth;

                paint2D.BeginPath();
                paint2D.MoveTo(new Vector2(0f, EvaluateCurveForPixel(0f)));

                int pixelWidth = (int)influenceCurveView.resolvedStyle.width;
                for (int i = 0; i <= pixelWidth; i++)
                {
                    Vector2 pixelCoord = new(i, EvaluateCurveForPixel(i));
                    // GetGraphCoordOfPixelCoord(pixelCoord, out var graphCoord);
                    paint2D.LineTo(pixelCoord);
                }

                paint2D.LineTo(new Vector2(influenceCurveView.resolvedStyle.width,
                    EvaluateCurveForPixel(influenceCurveView.resolvedStyle.width)));
                paint2D.Stroke();
            }
        }

        private bool GetPixelCoordOfGraphCoord(Vector2 graphCoord, out Vector2 pixelCoord)
        {
            float coordWidth = max.x - min.x;
            float coordHeight = max.y - min.y;
            float pixelWidth = influenceCurveView.resolvedStyle.width;
            float pixelHeight = influenceCurveView.resolvedStyle.height;

            Vector2 coordRatioInVisible = new()
            {
                x = graphCoord.x / coordWidth,
                y = graphCoord.y / coordHeight
            };

            pixelCoord = new Vector2
            {
                x = coordRatioInVisible.x * pixelWidth,
                y = pixelHeight - coordRatioInVisible.y * pixelHeight
            };

            pixelCoord.x = Mathf.Clamp(pixelCoord.x, 0f, pixelWidth);
            pixelCoord.y = Mathf.Clamp(pixelCoord.y, 0f, pixelHeight);

            return true;
        }

        private bool GetGraphCoordOfPixelCoord(Vector2 pixelCoord, out Vector2 graphCoord)
        {
            float coordWidth = max.x - min.x;
            float coordHeight = max.y - min.y;
            float pixelWidth = influenceCurveView.resolvedStyle.width;
            float pixelHeight = influenceCurveView.resolvedStyle.height;

            pixelCoord.y = pixelHeight - pixelCoord.y;

            Vector2 coordRatioInVisible = new()
            {
                x = pixelCoord.x / pixelWidth,
                y = pixelCoord.y / pixelHeight
            };

            graphCoord = new Vector2
            {
                x = coordRatioInVisible.x * coordWidth,
                y = coordRatioInVisible.y * coordHeight
            };

            return true;
        }

        private float EvaluateCurveForPixel(float pixelX)
        {
            if (GetGraphCoordOfPixelCoord(new Vector2(pixelX, 0f), out Vector2 graphCoord))
            {
                // Debug.Log($"EvaluateCurveForPixel exponent: {value.Exponent}");
                graphCoord.y = CalculateScore(graphCoord.x);
                if (GetPixelCoordOfGraphCoord(graphCoord, out Vector2 pixelCoord)) return pixelCoord.y;
            }

            return 0f;
        }

        #endregion

    }
}
﻿/*
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace SongChartVisualizer.Core
{
	public class WindowGraph : MonoBehaviour
	{
		private static readonly Color _defaultLinkColor = new Color(1, 1, 1, .5f);

		private RectTransform? _labelTemplateX;
		private RectTransform? _labelTemplateY;
		private RectTransform? _dashTemplateX;
		private RectTransform? _dashTemplateY;

		public Sprite? circleSprite;

		public RectTransform? GraphContainer { get; private set; }
		public List<GameObject>? DotObjects { get; private set; }
		public List<GameObject>? LinkObjects { get; private set; }
		public List<GameObject>? LabelXObjects { get; private set; }
		public List<GameObject>? LabelYObjects { get; private set; }
		public List<GameObject>? DashXObjects { get; private set; }
		public List<GameObject>? DashYObjects { get; private set; }

		private void Awake()
		{
			GraphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
			_labelTemplateX = GraphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
			_labelTemplateY = GraphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
			_dashTemplateX = GraphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
			_dashTemplateY = GraphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

			DotObjects = new List<GameObject>();
			LinkObjects = new List<GameObject>();
			LabelXObjects = new List<GameObject>();
			LabelYObjects = new List<GameObject>();
			DashXObjects = new List<GameObject>();
			DashYObjects = new List<GameObject>();
		}

		public void ShowGraph(List<float> valueList, bool makeDotsVisible = true, bool makeLinksVisible = true, bool makeOriginZero = false, int maxVisibleValueAmount = -1,
			Func<float, string>? getAxisLabelX = null, Func<float, string>? getAxisLabelY = null, Color? linkColor = null)
		{
			getAxisLabelX ??= i => i.ToString(CultureInfo.InvariantCulture);

			getAxisLabelY ??= f => Mathf.RoundToInt(f).ToString();

			if (maxVisibleValueAmount <= 0)
			{
				maxVisibleValueAmount = valueList.Count;
			}

			if (DotObjects != null)
			{
				foreach (var go in DotObjects)
				{
					Destroy(go);
				}

				DotObjects.Clear();
			}

			if (LinkObjects != null)
			{
				foreach (var go in LinkObjects)
				{
					Destroy(go);
				}

				LinkObjects.Clear();
			}

			if (LabelXObjects != null)
			{
				foreach (var go in LabelXObjects)
				{
					Destroy(go);
				}

				LabelXObjects.Clear();
			}

			if (LabelYObjects != null)
			{
				foreach (var go in LabelYObjects)
				{
					Destroy(go);
				}

				LabelYObjects.Clear();
			}

			if (DashXObjects != null)
			{
				foreach (var go in DashXObjects)
				{
					Destroy(go);
				}

				DashXObjects.Clear();
			}

			if (DashYObjects != null)
			{
				foreach (var go in DashYObjects)
				{
					Destroy(go);
				}

				DashYObjects.Clear();
			}

			var graphWidth = GraphContainer.sizeDelta.x;
			var graphHeight = GraphContainer.sizeDelta.y;

			var yMaximum = valueList[0];
			var yMinimum = valueList[0];

			for (var i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
			{
				var value = valueList[i];
				if (value > yMaximum)
				{
					yMaximum = value;
				}

				if (value < yMinimum)
				{
					yMinimum = value;
				}
			}

			var yDifference = yMaximum - yMinimum;
			if (yDifference <= 0)
			{
				yDifference = 5f;
			}

			yMaximum += (yDifference * 0.2f);
			yMinimum -= (yDifference * 0.2f);

			if (makeOriginZero)
			{
				yMinimum = 0f; // Start the graph at zero
			}

			var xSize = graphWidth / (maxVisibleValueAmount + 1);
			var xIndex = 0;

			linkColor = linkColor == null ? _defaultLinkColor : new Color(linkColor.Value.r, linkColor.Value.g, linkColor.Value.b, .5f);

			GameObject? lastCircleGameObject = null;
			for (var i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
			{
				var xPosition = xSize + xIndex * xSize;
				var yPosition = (valueList[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
				var circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), makeDotsVisible);
				DotObjects.Add(circleGameObject);
				if (lastCircleGameObject != null)
				{
					var dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
						circleGameObject.GetComponent<RectTransform>().anchoredPosition,
						makeLinksVisible,
						linkColor.Value);
					LinkObjects.Add(dotConnectionGameObject);
				}

				lastCircleGameObject = circleGameObject;

				var labelX = Instantiate(_labelTemplateX);
				labelX.SetParent(GraphContainer, false);
				labelX.gameObject.SetActive(true);
				labelX.anchoredPosition = new Vector2(xPosition, -7f);
				labelX.GetComponent<Text>().text = getAxisLabelX(i);
				LabelXObjects.Add(labelX.gameObject);

				var dashX = Instantiate(_dashTemplateX);
				dashX.SetParent(GraphContainer, false);
				dashX.gameObject.SetActive(true);
				dashX.anchoredPosition = new Vector2(yPosition, -3);
				DashXObjects.Add(dashX.gameObject);

				xIndex++;
			}

			var separatorCount = 10;
			for (var i = 0; i <= separatorCount; i++)
			{
				var labelY = Instantiate(_labelTemplateY);
				labelY.SetParent(GraphContainer, false);
				labelY.gameObject.SetActive(true);
				var normalizedValue = i * 1f / separatorCount;
				labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
				labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
				LabelYObjects.Add(labelY.gameObject);

				var dashY = Instantiate(_dashTemplateY);
				dashY.SetParent(GraphContainer, false);
				dashY.gameObject.SetActive(true);
				dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
				DashYObjects.Add(dashY.gameObject);
			}
		}

		private GameObject CreateCircle(Vector2 anchoredPosition, bool makeDotsVisible)
		{
			var go = new GameObject("Circle", typeof(ImageView));
			go.transform.SetParent(GraphContainer, false);
			var image = go.GetComponent<ImageView>();
			image.sprite = circleSprite;
			image.useSpriteMesh = true;
			image.enabled = makeDotsVisible;

			var rectTransform = go.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.sizeDelta = new Vector2(8, 8);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);

			return go;
		}

		private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, bool makeLinkVisible, Color linkColor)
		{
			var go = new GameObject("DotConnection", typeof(ImageView));
			go.transform.SetParent(GraphContainer, false);

			var image = go.GetComponent<ImageView>();
			image.color = linkColor;
			image.enabled = makeLinkVisible;

			var rectTransform = go.GetComponent<RectTransform>();
			var dir = (dotPositionB - dotPositionA).normalized;
			var distance = Vector2.Distance(dotPositionA, dotPositionB);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);
			rectTransform.sizeDelta = new Vector2(distance, 2f);
			rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
			rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

			return go;
		}
	}
}
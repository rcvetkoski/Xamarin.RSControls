﻿using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSPopup2
    {
        private RSPopup2()
        {

        }


        private static RSPopup2 instance;
        public static RSPopup2 GetInstance()
        {
            if (instance == null)
                instance = new RSPopup2();
            return instance;
        }

        public double X, Y;

        public void CretaeFragment()
        {
            
        }
    }

    public class RSPopup
    {
        private IDialogPopup service;
        public string Title { get; set; }
        public string Message { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float BorderRadius { get; set; }
        public Forms.Color BackgroundColor { get; set; }
        public float DimAmount { get; set; }
        public bool ShadowEnabled { get; set; }
        public bool IsModal { get; set; }



        public RSPopup()
        {
            service = DependencyService.Get<IDialogPopup>(DependencyFetchTarget.GlobalInstance);
            SetBackgroundColor(Color.White);
            SetBorderRadius(18);
            SetDimAmount(0.7f);
            SetShadowEnabled(true);
        }

        public RSPopup(string title, string message)
        {
            service = DependencyService.Get<IDialogPopup>(DependencyFetchTarget.GlobalInstance);
            SetTitle(title);
            SetMessage(message);
            SetBackgroundColor(Color.White);
            SetBorderRadius(18);
            SetDimAmount(0.7f);
            SetShadowEnabled(true);
        }

        public void Show()
        {
            //RSPopup2 lll = RSPopup2.GetInstance();

            service.ShowPopup();
        }

        public void SetPopupAtPosition(int x, int y)
        {
            PositionX = x;
            PositionY = y;

            service.PositionX = x;
            service.PositionY = y;

            service.UserSetPosition = true;
        }

        public void SetPopupPositionRelativeTo(View view)
        {
            service.RelativeView = view;
            service.UserSetPosition = true;
            service.RSPopupPositionSideEnum = RSPopupPositionSideEnum.Bottom; //default
        }

        public void SetPopupPositionRelativeTo(View view, RSPopupPositionSideEnum rSPopupPositionSideEnum)
        {
            service.RelativeView = view;
            service.UserSetPosition = true;
            service.RSPopupPositionSideEnum = rSPopupPositionSideEnum;
        }

        public void SetPopupSize(int width, int height)
        {
            Width = width;
            Height = height;

            service.Width = width;
            service.Height = height;
            service.UserSetSize = true;
        }

        public void SetPopupSize(int width, RSPopupSizeEnum height)
        {
            Width = width;
            Height = (int)height;

            service.Width = Width;
            service.Height = Height;
            service.UserSetSize = true;
        }

        public void SetPopupSize(RSPopupSizeEnum width, int height)
        {
            Width = (int)width;
            Height = height;

            service.Width = Width;
            service.Height = Height;
            service.UserSetSize = true;
        }

        public void SetPopupSize(RSPopupSizeEnum width, RSPopupSizeEnum height)
        {
            Width = (int)width;
            Height = (int)height;

            service.Width = Width;
            service.Height = Height;
            service.UserSetSize = true;
        }

        public void SetTitle(string title)
        {
            Title = title;
            service.Title = this.Title;
        }

        public void SetMessage(string message)
        {
            Message = message;
            service.Message = this.Message;
        }

        public void SetCustomView(Forms.View customView)
        {
            service.CustomView = customView;
        }

        public void SetBorderRadius(float borderRadius)
        {
            BorderRadius = borderRadius;
            service.BorderRadius = this.BorderRadius;
        }

        public void SetBackgroundColor(Forms.Color color)
        {
            BackgroundColor = color;
            service.BorderFillColor = BackgroundColor;
        }

        public void SetShadowEnabled(bool enabled)
        {
            ShadowEnabled = enabled;
            service.ShadowEnabled = this.ShadowEnabled;
        }

        public void SetIsModal(bool isModal)
        {
            IsModal = isModal;
            service.IsModal = isModal;
        }


        public void SetDimAmount(float amount)
        {
            DimAmount = amount;
            service.DimAmount = DimAmount;
        }

        public void SetPopupAnimation(RSPopupAnimationEnum rSPopupAnimationEnum = RSPopupAnimationEnum.Default)
        {
            //TODO
        }

        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command = null, object commandParameter = null)
        {
            service.AddAction(title, rSPopupButtonType, command, commandParameter);
        }

        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, EventHandler handler)
        {
            service.AddAction(title, rSPopupButtonType, null, null);
        }
    }
}

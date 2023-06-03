using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class UISpriteAnimation : MonoBehaviour
{
    private Image ImageSource;
    private int mCurFrame = 0;
    private float mDelta = 0;

    public float FPS = 5;
    public bool IsPlaying = false;
    public bool Foward = true;
    public bool AutoPlay = true;
    public bool Loop = true;
    public bool isNativeSize = false;

    private List<Sprite> mSpriteFrames;

    public int FrameCount
    {
        get
        {
            return mSpriteFrames.Count;
        }
    }

    public List<Sprite> Sprites
    {
        get
        {
            return mSpriteFrames;
        }
        set
        {
            mSpriteFrames = value;
            if (mSpriteFrames.Count > 0)
            {
                SetSprite(0);
            }

            StartPlay();
        }
    }
    void Awake()
    {
        ImageSource = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartPlay();
    }

    private void OnDisable()
    {
        Stop();
    }

    void StartPlay()
    {
        if (AutoPlay)
        {
            Play();
        }
        else
        {
            IsPlaying = false;
        }
    }

    private void SetSprite(int idx)
    {
        if (ImageSource == null)
        {
            ImageSource = GetComponent<Image>();
        }
        ImageSource.sprite = mSpriteFrames[idx];
        if (isNativeSize)
        {
            ImageSource.SetNativeSize();
        }
    }

    public void Play()
    {
        IsPlaying = true;
        Foward = true;
    }

    public void PlayReverse()
    {
        IsPlaying = true;
        Foward = false;
    }

    void Update()
    {
        if (!IsPlaying ||  FrameCount <= 1)
        {
            return;
        }

        mDelta += Time.deltaTime;
        if (mDelta > 1 / FPS)
        {
            mDelta = 0;
            if (Foward)
            {
                mCurFrame++;
            }
            else
            {
                mCurFrame--;
            }

            if (mCurFrame >= FrameCount)
            {
                if (Loop)
                {
                    mCurFrame = 0;
                }
                else
                {
                    IsPlaying = false;
                    return;
                }
            }
            else if (mCurFrame < 0)
            {
                if (Loop)
                {
                    mCurFrame = FrameCount - 1;
                }
                else
                {
                    IsPlaying = false;
                    return;
                }
            }

            SetSprite(mCurFrame);
        }
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Resume()
    {
        if (!IsPlaying)
        {
            IsPlaying = true;
        }
    }

    public void Stop()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        IsPlaying = false;
    }

    public void Rewind()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        Play();
    }
}
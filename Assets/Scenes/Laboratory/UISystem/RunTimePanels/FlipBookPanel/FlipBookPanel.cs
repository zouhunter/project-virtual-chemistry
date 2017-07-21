using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using BundleUISystem;

public class FlipBookPanel :UIPanelTemp
{
    public Button openBtn;
    public Button closeButton;

    public Book m_Book;
    private AutoFlip m_Flip;

    public GameObject panel;
    private List<Sprite> sprites;
    void Start()
    {
        closeButton.onClick.AddListener(delegate()
        {
            Destroy(gameObject);
        });

        panel.gameObject.SetActive(false);
        m_Flip = m_Book.GetComponent<AutoFlip>();

        openBtn.onClick.AddListener(ShowBook);
        openBtn.onClick.Invoke();
    }

    private void ShowBook()
    {
        if (sprites!=null)
        {
            panel.gameObject.SetActive(true);
            m_Book.canvas = transform.root.GetComponent<Canvas>();
            m_Book.currentPage = 1;
            m_Book.bookPages = new Sprite[sprites.Count];
            sprites.CopyTo(m_Book.bookPages);
            m_Book.InitBook();
            m_Flip.InitFlip();
        }
        else
        {
            DownLandUtility.DownLandSprites("Book", (x) => { sprites = x; ShowBook(); });
        }
    }
}

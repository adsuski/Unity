using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class Bird : MonoBehaviour
{
    private const float WARTOSC_SKOKU = 100f;
    

    private static Bird instance;

    public static Bird GetInstance()
    {
        return instance;
    }

    public event EventHandler Smierc;
    public event EventHandler RozpoczecieGry;




    private Rigidbody2D ptakrigidbody2D;
    private State state;

    private enum State
    {
        Oczekiwanie,
        Gra,
        KoniecGry
    }

    private void Awake()
    {
        instance = this;
        ptakrigidbody2D = GetComponent<Rigidbody2D>();
        ptakrigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.Oczekiwanie;

    }

   private void Update() {
        switch (state)
        {
            default:
            case State.Oczekiwanie:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    state = State.Gra;
                    ptakrigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                    Jump();
                    if (RozpoczecieGry != null) RozpoczecieGry(this, EventArgs.Empty);
                }
                break;
            case State.Gra:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    Jump();
                }
                break;
            case State.KoniecGry:
                break;
        }
        
    }

    private void Jump()
    {
        ptakrigidbody2D.velocity = Vector2.up * WARTOSC_SKOKU;
        SoundManager.PlaySound();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        
        ptakrigidbody2D.bodyType = RigidbodyType2D.Static;
        if (Smierc != null) Smierc(this, EventArgs.Empty);
    }

}

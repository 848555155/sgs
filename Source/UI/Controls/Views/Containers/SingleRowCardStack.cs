﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Sanguosha.UI.Controls;

public class SingleRowCardStack : CardStack
{
    public SingleRowCardStack()
    {
        cardBeginDragHandler = new EventHandler(card_OnDragBegin);
        cardDraggingHandler = new EventHandler(card_OnDragging);
        cardEndDragHandler = new EventHandler(card_OnDragEnd);
        cardMouseEnterHandler = new MouseEventHandler(card_MouseEnter);
        cardMouseLeaveHandler = new MouseEventHandler(card_MouseLeave);
        IsDraggingHandled = true;
        registeredCards = new HashSet<CardView>();
        this.Unloaded += SingleRowCardStack_Unloaded;
    }

    private void SingleRowCardStack_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        foreach (var card in registeredCards)
        {
            card.OnDragBegin -= cardBeginDragHandler;
            card.OnDragging -= cardDraggingHandler;
            card.OnDragEnd -= cardEndDragHandler;
            card.MouseEnter -= cardMouseEnterHandler;
            card.MouseLeave -= cardMouseLeaveHandler;
        }
        registeredCards.Clear();
    }

    public bool IsDraggingHandled
    {
        get;
        set;
    }

    private readonly MouseEventHandler cardMouseLeaveHandler;
    private readonly MouseEventHandler cardMouseEnterHandler;
    private readonly EventHandler cardBeginDragHandler;
    private readonly EventHandler cardDraggingHandler;
    private readonly EventHandler cardEndDragHandler;
    private readonly HashSet<CardView> registeredCards;

    protected override void RegisterCardEvents(CardView card)
    {
        card.OnDragBegin += cardBeginDragHandler;
        card.OnDragging += cardDraggingHandler;
        card.OnDragEnd += cardEndDragHandler;
        card.MouseEnter += cardMouseEnterHandler;
        card.MouseLeave += cardMouseLeaveHandler;
        registeredCards.Add(card);
    }

    protected override void UnregisterCardEvents(CardView card)
    {
        if (card == InteractingCard)
        {
            if (CardStatus == CardInteraction.Drag)
            {
                card_OnDragEndUnlock();
            }
            if (CardStatus == CardInteraction.MouseMove)
            {
                card_MouseLeaveUnlock();
            }
        }
        card.OnDragBegin -= cardBeginDragHandler;
        card.OnDragging -= cardDraggingHandler;
        card.OnDragEnd -= cardEndDragHandler;
        card.MouseEnter -= cardMouseEnterHandler;
        card.MouseLeave -= cardMouseLeaveHandler;
        registeredCards.Remove(card);
    }

    #region Drag and Drop, Highlighting
    private void card_MouseLeave(object sender, MouseEventArgs e)
    {
        if (CardStatus == CardInteraction.MouseMove)
        {
            lock (Cards)
            {
                Trace.TraceInformation("MouseLeave");
                card_MouseLeaveUnlock();
            }
        }
    }

    private void card_MouseLeaveUnlock()
    {
        CardStatus = CardInteraction.None;
        InteractingCard = null;
        RearrangeCards();
    }

    private void card_MouseEnter(object sender, MouseEventArgs e)
    {
        if (CardStatus == CardInteraction.None)
        {
            lock (Cards)
            {
                Trace.TraceInformation("MouseEnter");
                InteractingCard = sender as CardView;
                if (InteractingCard != null)
                {
                    CardStatus = CardInteraction.MouseMove;
                    RearrangeCards();
                }
            }
        }
    }

    private void card_OnDragEndUnlock()
    {
        if (!IsDraggingHandled) return;
        Trace.TraceInformation("DragEnd");
        int newPos = InteractingCardIndex;
        CardStatus = CardInteraction.None;            
        int oldPos = Cards.IndexOf(InteractingCard);
        if (newPos != oldPos)
        {
            Cards.Remove(InteractingCard);
            Cards.Insert(newPos, InteractingCard);
            var handler = OnHandCardMoved;
            if (handler != null)
            {
                handler(oldPos, newPos);
            }
        }
        RearrangeCards();
    }

    public event HandCardMovedHandler OnHandCardMoved;

    private void card_OnDragEnd(object sender, EventArgs e)
    {
        if (!IsDraggingHandled) return;
        if (CardStatus == CardInteraction.Drag)
        {
            lock (Cards)
            {
                card_OnDragEndUnlock();
            }
            CardStatus = CardInteraction.MouseMove;
        }
    }

    private void card_OnDragging(object sender, EventArgs e)
    {
        if (!IsDraggingHandled) return;
        if (CardStatus == CardInteraction.Drag)
        {
            lock (Cards)
            {
                RearrangeCards();
            }
        }
    }

    private void card_OnDragBegin(object sender, EventArgs e)
    {
        if (!IsDraggingHandled) return;
        if (CardStatus == CardInteraction.MouseMove)
        {
            lock (Cards)
            {
                Trace.TraceInformation("DragBegin");
                InteractingCard = sender as CardView;
                InteractingCard.SetValue(ZIndexProperty, 1000);
                Trace.Assert(InteractingCard != null);
                CardStatus = CardInteraction.Drag;
                RearrangeCards();
            }
        }
    }
    #endregion
}

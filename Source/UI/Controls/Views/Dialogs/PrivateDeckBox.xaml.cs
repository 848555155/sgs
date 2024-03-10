using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls;

/// <summary>
/// Interaction logic for PrivateDeckBox.xaml
/// </summary>
public partial class PrivateDeckBox : UserControl
{
    public PrivateDeckBox()
    {
        InitializeComponent();
        cardStack.ParentCanvas = cardCanvas;
        this.DataContextChanged += new DependencyPropertyChangedEventHandler(PrivateDeckBox_DataContextChanged);
    }

    private void PrivateDeckBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var model = DataContext as IList<CardViewModel>;
        if (model == null) return;
        this.Width = Math.Min(model.Count * 93, 500);
        foreach (var card in model)
        {
            CardView view = new CardView(card) { Opacity = 1.0 };
            cardStack.Cards.Add(view);
        }
        cardStack.RearrangeCards();
    }
}

using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sanguosha.UI.Controls;

public class GuHuoSkillCommand : SkillCommand
{
    public GuHuoSkillCommand()
    {
        guHuoTypes = new ObservableCollection<CardHandler>();
        guHuoCommand = new GuHuoChoiceCommand() { ParentSkillCommand = this };
    }

    private class GuHuoChoiceCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        private GuHuoSkillCommand parentSkillCommand;

        public GuHuoSkillCommand ParentSkillCommand
        {
            get { return parentSkillCommand; }
            set { parentSkillCommand = value; }
        }

        public void Execute(object parameter)
        {
            CardHandler selection = parameter as CardHandler;
            parentSkillCommand.GuHuoChoice = selection;
        }
    }

    private readonly GuHuoChoiceCommand guHuoCommand;

    public ICommand GuHuoCommand
    {
        get { return guHuoCommand; }
    }

    public CardHandler GuHuoChoice
    {
        get
        {
            if (Skill is IAdditionalTypedSkill skill)
            {
                return skill.AdditionalType;
            }
            return null;
        }
        set
        {
            if (Skill is IAdditionalTypedSkill skill)
            {
                if (skill.AdditionalType != value)
                {
                    skill.AdditionalType = value;
                    SelectedChanged();
                }
            }
        }
    }

    private ObservableCollection<CardHandler> guHuoTypes;

    public ObservableCollection<CardHandler> GuHuoTypes
    {
        get { return guHuoTypes; }
        private set { guHuoTypes = value; }
    }
}

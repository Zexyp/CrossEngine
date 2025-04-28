using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngineEditor.Modals;
using CrossEngineEditor.Utils.UI;
using ImGuiNET;
using Silk.NET.Input;

namespace CrossEngineEditor.Panels;

public class PanelManager
{
    public readonly ReadOnlyCollection<EditorPanel> Registered; 
    
    readonly List<EditorPanel> _panels = new List<EditorPanel>();
    readonly List<EditorPanel> _registeredPanels = new List<EditorPanel>();
    readonly LinkedList<EditorModal> _modals = new LinkedList<EditorModal>();
    //readonly List<Popup> _popups = new List<Popup>();
    readonly Logger _log = new Logger("panel-manager");
    private IEditorContext _context;
    private bool _initialized = false;

    public PanelManager()
    {
        Registered = _panels.AsReadOnly();
    }

    public void Init(IEditorContext context)
    {
        _context = context;
        _initialized = true;
        
        for (int i = 0; i < _panels.Count; i++)
        {
            AttachPanel(_panels[i]);
        }
    }

    public void Destroy()
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            DetachPanel(_panels[i]);
        }
        
        _initialized = false;
    }

    public void RegisterPanel(EditorPanel panel)
    {
        Debug.Assert(!_registeredPanels.Contains(panel));

        if (_registeredPanels.Contains(panel)) throw new InvalidOperationException();

        _registeredPanels.Add(panel);

        PushPanel(panel);

        _log.Trace($"registered panel '{panel.GetType().FullName}'");
    }

    public void UnregisterPanel(EditorPanel panel)
    {
        Debug.Assert(_registeredPanels.Contains(panel));

        if (!_registeredPanels.Contains(panel)) throw new InvalidOperationException();

        _registeredPanels.Remove(panel);

        RemovePanel(panel);

        _log.Trace($"unregistered panel '{panel.GetType().FullName}'");
    }

    public void PushPanel(EditorPanel panel)
    {
        Debug.Assert(!_panels.Contains(panel));

        _panels.Add(panel);
        panel.Context = _context;

        if (_initialized)
            AttachPanel(panel);
        
        _log.Trace($"pushed panel '{panel.GetType().FullName}'");
    }

    public void RemovePanel(EditorPanel panel)
    {
        Debug.Assert(_panels.Contains(panel));

        if (_initialized)
            DetachPanel(panel);
        
        panel.Context = null;
        _panels.Remove(panel);
        
        _log.Trace($"removed panel '{panel.GetType().FullName}'");
    }

    public void PushModal(EditorModal modal)
    {
        if (!_initialized)
            throw new InvalidOperationException("pls init");

        Debug.Assert(modal.Open == true || modal.Open == null);

        _modals.AddLast(modal);
        
        _log.Trace($"pushed modal '{modal.GetType().FullName}'");
    }
    
    //public void PushPopup(Popup popup)
    //{
    //    if (!_initialized)
    //        throw new InvalidOperationException("pls init");
    //    
    //    _popups.Add(popup);
    //    
    //    _log.Trace($"pushed popup '{popup.GetType().FullName}'");
    //}

    public T GetPanel<T>() where T : EditorPanel
    {
        return (T)GetPanel(typeof(T));
    }
    
    public EditorPanel GetPanel(Type typeOfPanel)
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            if (_panels[i].GetType() == typeOfPanel)
                return _panels[i];
        }
        return null;
    }

    public T GetModal<T>() where T : EditorModal
    {
        return (T)GetModal(typeof(T));
    }

    public EditorModal GetModal(Type typeOfModal)
    {
        for (var node = _modals.First; node != null; node = node.Next)
        {
            if (node.Value.GetType() == typeOfModal)
                return node.Value;
        }
        return null;
    }

    public void Draw()
    {
        Profiler.BeginScope();
        
        DrawPanels();
        DrawModals();
        //DrawPopups();
        
        Profiler.EndScope();
    }
    
    private void DrawPanels()
    {
        Profiler.BeginScope();

        for (int i = 0; i < _panels.Count; i++)
        {
            var p = _panels[i];

            Profiler.BeginScope($"{p.GetType().Name}.{nameof(EditorPanel.Draw)}");
            try
            {
                p.Draw();
                if (p.Open == false && !_registeredPanels.Contains(p))
                {
                    RemovePanel(p);
                    i--;
                }
            }
            catch (Exception e)
            {
                _log.Error(
                    $"incident while drawing a panel '{p.WindowName}' ({p.GetType().FullName}): {e.GetType().FullName}: {e.Message}");
                throw;
            }
            finally
            {
                Profiler.EndScope();
            }
        }
        
        Profiler.EndScope();
    }
    
    private void DrawModals()
    {
        Profiler.BeginScope();

        void DrawModal(LinkedListNode<EditorModal> lln)
        {
            if (lln == null) return;

            var m = lln.Value;
            try
            {
                m.Draw(() => DrawModal(lln.Next));
                if (m.Open == false)
                {
                    _log.Trace($"modal closed '{m.GetType().FullName}'");
                    _modals.Remove(m);
                }
            }
            catch (Exception e)
            {
                _log.Error($"incident while drawing a modal '{m.ModalName}' ({m.GetType().FullName}): {e.GetType().FullName}: {e.Message}");
                throw;
            }
        }

        DrawModal(_modals.First);
        
        Profiler.EndScope();
    }

    private void AttachPanel(EditorPanel panel)
    {
        panel.OnAttach();
        
        if (panel.Open != false) panel.OnOpen();
        
        _log.Trace($"attached panel '{panel.GetType().FullName}'");
    }

    private void DetachPanel(EditorPanel panel)
    {
        if (panel.Open != false) panel.OnClose();
        
        panel.OnDetach();
        
        _log.Trace($"detached panel '{panel.GetType().FullName}'");
    }
}
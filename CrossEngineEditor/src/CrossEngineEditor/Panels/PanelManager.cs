using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CrossEngine.Logging;
using CrossEngineEditor.Modals;
using ImGuiNET;

namespace CrossEngineEditor.Panels;

public class PanelManager
{
    public readonly ReadOnlyCollection<EditorPanel> Registered; 
    
    readonly List<EditorPanel> _panels = new List<EditorPanel>();
    readonly List<EditorPanel> _registeredPanels = new List<EditorPanel>();
    readonly List<EditorModal> _modals = new List<EditorModal>();
    readonly Logger _log = new Logger("panels");
    private IEditorContext _context;
    private bool _initialized = false;

    public PanelManager()
    {
        Registered = _panels.AsReadOnly();
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
        if (_registeredPanels.Contains(panel)) throw new InvalidOperationException();

        _registeredPanels.Add(panel);

        PushPanel(panel);

        _log.Trace($"registered panel '{panel.GetType().FullName}'");
    }

    public void UnregisterPanel(EditorPanel panel)
    {
        if (!_registeredPanels.Contains(panel)) throw new InvalidOperationException();

        _registeredPanels.Remove(panel);

        RemovePanel(panel);

        _log.Trace($"unregistered panel '{panel.GetType().FullName}'");
    }

    public void PushPanel(EditorPanel panel)
    {
        _panels.Add(panel);
        panel.Context = _context;

        if (_initialized)
            AttachPanel(panel);
        
        _log.Trace($"pushed panel '{panel.GetType().FullName}'");
    }

    public void RemovePanel(EditorPanel panel)
    {
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
        
        _modals.Add(modal);
        
        _log.Trace($"pushed modal '{modal.GetType().FullName}'");
    }

    //public T GetPanel<T>() where T : EditorPanel
    //{
    //    return (T)GetPanel(typeof(T));
    //}
    //
    //public EditorPanel GetPanel(Type typeOfPanel)
    //{
    //    for (int i = 0; i < _panels.Count; i++)
    //    {
    //        if (_panels[i].GetType() == typeOfPanel)
    //            return _panels[i];
    //    }
    //    return null;
    //}

    public void Draw()
    {
        DrawPanels();
        DrawModals();
    }
    
    private void DrawPanels()
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            var p = _panels[i];

            try
            {
                p.Draw();
            }
            catch (Exception e)
            {
                _log.Error($"incident while drawing a panel '{p.WindowName}' ({p.GetType().FullName}): {e.GetType().FullName}: {e.Message}");
                throw;
            }
        }
    }
    
    private void DrawModals()
    {
        for (int i = 0; i < _modals.Count; i++)
        {
            var m = _modals[i];

            try
            {
                if (!m.Draw())
                {
                    _log.Trace($"modal closed '{m.GetType().FullName}'");
                    _modals.Remove(m);
                    i--;
                }
            }
            catch (Exception e)
            {
                _log.Error($"incident while drawing a modal '{m.ModalName}' ({m.GetType().FullName}): {e.GetType().FullName}: {e.Message}");
                throw;
            }
        }
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
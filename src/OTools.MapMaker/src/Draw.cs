﻿using Avalonia.Input;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OTools.MapMaker;

public class MapDraw
{
    private (PointDraw point, SimplePathDraw sPath) draws;

    private bool isActive;
    private Active active;

    public MapDraw()
    {
        Manager.ActiveSymbolChanged += SymbolChanged;
        Manager.ActiveToolChanged += args => isActive = (args != Tool.Edit);

        //ViewManager.MouseDown += args => MouseDown(args.)

        ViewManager.MouseUp += args => MouseUp(args.InitialPressMouseButton);
        ViewManager.MouseMove += _ => MouseMove();

        ViewManager.KeyUp += args => KeyUp(args.Key);
    }
    
    public void SymbolChanged(Symbol sym)
    {

        switch (sym)
        {
            case PointSymbol point:
                active = Active.Point;
                draws.point = new(point);
                draws.point.Start();
                break;
            case IPathSymbol path:
                active = Active.SimplePath;
                draws.sPath = new(path);
                break;
        }
    }

    public void MouseDown(MouseButton mouse)
    {
        
    }

    public void MouseUp(MouseButton mouse)
    {
        switch (active)
        {
            case Active.Point: if (mouse == MouseButton.Left) draws.point.End(); break;
            case Active.SimplePath:
                if (mouse == MouseButton.Left)
                {
                    draws.sPath.Start();
                    draws.sPath.NewPoint();
                }
                else if (mouse == MouseButton.Right)
                {
                    draws.sPath.End();
                }

                break;
        }
    }

    public void MouseMove()
    {
        switch (active)
        {
            case Active.Point: draws.point.Update(); break;
            case Active.SimplePath: 
                draws.sPath.Update(); 
                draws.sPath.Idle(); 
                break;  
        }
    }

    public void KeyUp(Key key)
    {
        switch (active)
        {
            case Active.SimplePath:
                switch (key)
                {
                    case Key.Enter:
                        draws.sPath.Complete();
                        break;
                    case Key.Escape:
                        draws.sPath.Cancel();
                        break;
                }
                break;
        }
    }

    private enum Active { Point, SimplePath, ComplexPath }
}

public class PointDraw
{
    private PointInstance _inst;
    private IMapRender _render;

    private bool _active;

    public PointDraw(PointSymbol sym)
    {
        _inst = new(Manager.Layer, sym, vec2.Zero, 0f);

        if (Manager.MapRender is null)
            Manager.MapRender = new MapRender(Manager.Map!);
        _render = Manager.MapRender;

        _active = false;
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst.Centre = ViewManager.MousePosition;
        _inst.Opacity = Manager.Settings.Draw_Opacity;

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _inst.Centre = ViewManager.MousePosition;

        //if (ViewManager.IsMouseOutsideBounds())
        //    _inst.Opacity = 0f;
        //else _inst.Opacity = 1f;

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _inst.Centre = ViewManager.MousePosition;
        _inst.Opacity = 1f;

        Manager.Map!.Instances.Add(_inst);

        var render = _render.RenderPointInstance(_inst);
        ViewManager.Update(_inst.Id, render);

        _inst = new(_inst.Layer, _inst.Symbol, _inst.Centre, _inst.Rotation);
        Start();
    }
}

public class SimplePathDraw
{
    private PathInstance _inst;
    private IMapRender _render;

    private bool _active;
    private List<vec2> _points;

	private bool _drawGuide;

    public SimplePathDraw(IPathSymbol sym)
    {
        _inst = sym switch
        {
            LineSymbol l => new LineInstance(Manager.Layer, l, new(), false, 0f),
            AreaSymbol a => new AreaInstance(Manager.Layer, a, new(), false, 0f),
            _ => throw new InvalidOperationException(),
        };

        if (Manager.MapRender is null)
            Manager.MapRender = new MapRender(Manager.Map!);
        _render = Manager.MapRender;

        _active = false;
        _points = new();
    }

    public void Start()
    {
        if (_active) return;
        _active = true;

        _inst = _inst.Clone();
        _points = new() { ViewManager.MousePosition, ViewManager.MousePosition };

        _inst.Segments.Reset(_points);
        _inst.Opacity = Manager.Settings.Draw_Opacity;
        _inst.IsClosed = false;

		_drawGuide = _inst is AreaInstance area &&
					 (area.Symbol.BorderWidth == 0 || area.Symbol.BorderColour == Colour.Transparent);

		var render = _render.RenderPathInstance(_inst).Concat(!_drawGuide ? Enumerable.Empty<IShape>() :
			new IShape[] { new Line {Colour = 0xffff8a00, Points = _points, Width = 1, ZIndex = 999}});
        ViewManager.Add(_inst.Id, render);
    }

    public void Update()
    {
        if (!_active) return;

        _points[^1] = ViewManager.MousePosition;

        _inst.Segments.Reset(_points);
        
		var render = _render.RenderPathInstance(_inst).Concat(!_drawGuide ? Enumerable.Empty<IShape>() :
			new IShape[] { new Area {BorderColour = Manager.Settings.Draw_BorderColour, Points = _points, BorderWidth = Manager.Settings.Draw_BorderWidth, ZIndex = Manager.Settings.Draw_BorderZIndex }});
        ViewManager.Update(_inst.Id, render);
    }

    public void NewPoint()
    {
        if (!_active) return;

        _points.Add(ViewManager.MousePosition);

        _inst.Segments.Reset(_points);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void End()
    {
        if (!_active) return;
        _active = false;

        _points.Add(ViewManager.MousePosition);

		if (vec2.Mag(_points[0], _points[^1]) < 1)
		{
			_active = true;
			Complete();
			return;
		}

        if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;

        Manager.Map!.Instances.Add(_inst);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);

        
    }

    public void Complete()
    {
        if (!_active) return;
        _active = false;

        _points.Remove(_points[^1]);
        //_points.Add(ViewManager.MousePosition);

		if (_points[0] == _points[1])
            _points.RemoveAt(1);

        _inst.Segments.Reset(_points);
        _inst.Opacity = 1f;
        _inst.IsClosed = true;

        Manager.Map!.Instances.Add(_inst);

        var render = _render.RenderPathInstance(_inst);
        ViewManager.Update(_inst.Id, render);
    }

    public void Idle()
    {
        
    }

    public void Cancel()
    {
        if (!_active) return;
        _active = false;

        _points = new();

        _inst = _inst.Clone();
        _inst.Segments.Clear();

        ViewManager.Remove(_inst.Id);
    }
}

file static class Extension
{
    public static void Reset(this PathCollection pC, IEnumerable<vec2> points)
    {
        pC.Clear();
        pC.Add(new LinearPath(points));
    }
}
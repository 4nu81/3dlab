//
// IntfFigures.cs 
//	- Interfaces to let Mainform in 3DGLLab "understand" your Modules
//
// Authors:
//	Andreas Maertens <mcmaerti@gmx.de>
//
// Copyright 2011 by Andreas Maertens


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GL3DLab
{
    /// <summary>
    /// Simulationsmuster für die Partikelfelder
    /// </summary>
    enum ParticleForms { universe, barrel, cones }

    /// <summary>
    /// Initialisiertungsformen für die Partikelfelder
    /// </summary>
    enum initForms { flat, universe, cube }

    /// <summary>
    /// Zeichenformen für die Partikelfelder
    /// </summary>
    enum DotStyle { Point, Vertects, Cubes, Rounds }

    /// <summary>
    /// Interface ermöglicht Kommunikation mit dem Objekt durch die Mainform. Schnittstelle für Darstellung und Steuerung der Module
    /// </summary>
    interface IFigures
    {
        /// <summary>
        /// Zeichnet das Objekt. Das Objekt muss alleine wissen, wie es gezeichnet wird.
        /// 'using Tao.OpenGl' muss verwendet werden;
        /// </summary>
        void Draw();
        /// <summary>
        /// Initialisiert das zu zeichnende Objekt
        /// </summary>
        void Init();
        /// <summary>
        /// diese Methode ist für Berechnungen neuer Positionen zuständig.
        /// </summary>
        void Tick();
        /// <summary>
        /// Reicht die KeyEvents and die Objekte weiter, die diese dann auswerten können.
        /// </summary>
        /// <param name="e">Das Key Event enthält die gedrückte Taste</param>
        void KeyPressed(KeyEventArgs e);
        Camera Cam { get; set; }
        bool Enabled { get; set; }
        bool Visible { get; set; }
        /// <summary>
        /// Falls Threads oder dergleichen im Objekt laufen müssen diese beendet werden.
        /// </summary>
        void OnShutdown();

        LightSource getDefaultLight();
        
    }

    /// <summary>
    /// Schnittstelle ermöglicht Maussteuerung und Mausklicks auszuwerten.
    /// </summary>
    interface IMouseControlled
    {
        /// <summary>
        /// Bewegung der Maus in X-Richtung
        /// </summary>
        double mx { get; set; }
        /// <summary>
        /// Bewegung der Maus in Y-Richtung
        /// </summary>
        double my { get; set; }
        /// <summary>
        /// Mousebutton geklickt
        /// </summary>
        /// <param name="e">Das Event enthält die gedrückte Taste</param>
        void MouseButton(MouseEventArgs e);
        /// <summary>
        /// aktualisieren nach MouseEvent
        /// </summary>
        void updateObject();
    }

    /// <summary>
    /// Basisklasse für alle Figuren, die Grundlegendes Verhalten steuert.
    /// </summary>
    public abstract class BaseFigure : IFigures
    {
        #region IFigures Member

        /// <summary>
        /// Interne Funktion zum Zeichnen des Objektes
        /// </summary>
        protected abstract void InternalDraw();
        /// <summary>
        /// Interne Funktion für den Tick des Objektes
        /// </summary>
        protected abstract void InternalTick();
        /// <summary>
        /// Jedes Objekt muss selbst wissen, wie es initialisiert wird.
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Reicht die KeyEvents and die Objekte weiter, die diese dann auswerten können.
        /// </summary>
        /// <param name="e">Das Key Event enthält die gedrückte Taste</param>
        public abstract void KeyPressed(KeyEventArgs e);
        /// <summary>
        /// Soll bei Visible die Interne Drawfunktion aufrufen.
        /// </summary>
        public void Draw()
        {
            if (Visible) InternalDraw();
        }
        /// <summary>
        /// Soll bei Enabled die Interne Tickfunktion aufrufen.
        /// </summary>
        public void Tick()
        {
            if (Enabled) InternalTick();
        }
        /// <summary>
        /// Wird beim Beenden der Mainform aufgerufen.
        /// </summary>        
        public abstract void OnShutdown();
        public LightSource getDefaultLight() 
        {
          return DefaultLight;
        }
        /// <summary>
        /// Somit kann die Klasse bei Bedarf auf die Kamera zugreifen.
        /// </summary>
        public Camera Cam { get; set; }
        /// <summary>
        /// Schalter für die Simulation des Objektes
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Schalter für die Sichtbarkeit des Objektes
        /// </summary>
        public bool Visible { get; set; }

        protected LightSource DefaultLight = LightSource.none;

        #endregion               
    }
}

//FECHA: 2024/10/12

/*
Este es un código hecho por FELIPE CORREA RODRÍGUEZ/FECORO.

El programa utiliza el algoritmo Minimax para simular un juego de ajedrez entre un jugador humano y la computadora.
Es una versión beta y aún no está terminada, pero se puede jugar con las piezas blancas y la computadora, aunque la IA no es muy buena.
Falta implementar funciones para que la IA pueda mover las piezas negras y mejorar la evaluación de la posición.

El código está en español y es un poco largo, pero está bien comentado y es fácil de entender.

Para jugar, simplemente ejecuta el programa y haz clic en una pieza blanca para seleccionarla y luego en una casilla válida para moverla.
La computadora moverá automáticamente las piezas negras después de que el jugador mueva una pieza blanca.

¡Espero que disfrutes jugando con este pequeño juego de ajedrez!

*/

//.............................................Importo
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


//.............................................Clase
public class TableroAjedrez : Form
{
    private Button[,] casillas = new Button[8, 8];
    private Pieza[,] piezas = new Pieza[8, 8];
    private List<Pieza> piezasBlancas = new List<Pieza>();
    private List<Pieza> piezasNegras = new List<Pieza>();
    private Pieza piezaSeleccionada = null;
    private bool turnoBlancas = true;
    private const int profundidadIA = 3;

    public TableroAjedrez()
    {
        InitializeComponents();
        InicializarTablero();
        InicializarPiezas();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(800, 800);
        this.Text = "FECHESS X";
        
        TableLayoutPanel tablero = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 8
        };

        for (int i = 0; i < 8; i++)
        {
            tablero.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
            tablero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
        }

        for (int fila = 0; fila < 8; fila++)
        {
            for (int col = 0; col < 8; col++)
            {
                casillas[fila, col] = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = (fila + col) % 2 == 0 ? Color.White : Color.Gray,
                    FlatStyle = FlatStyle.Flat
                };

                int filaCapturada = fila;
                int colCapturada = col;
                casillas[fila, col].Click += (sender, e) => CasillaClick(filaCapturada, colCapturada);
                tablero.Controls.Add(casillas[fila, col], col, fila);
            }
        }

        this.Controls.Add(tablero);
    }

    private void InicializarPiezas()
    {
        // Inicializar peones
        for (int i = 0; i < 8; i++)
        {
            piezas[1, i] = new Peon(false, 1, i);
            piezas[6, i] = new Peon(true, 6, i);
            piezasNegras.Add(piezas[1, i]);
            piezasBlancas.Add(piezas[6, i]);
        }

        // Inicializar piezas negras
        piezas[0, 0] = new Torre(false, 0, 0);
        piezas[0, 1] = new Caballo(false, 0, 1);
        piezas[0, 2] = new Alfil(false, 0, 2);
        piezas[0, 3] = new Reina(false, 0, 3);
        piezas[0, 4] = new Rey(false, 0, 4);
        piezas[0, 5] = new Alfil(false, 0, 5);
        piezas[0, 6] = new Caballo(false, 0, 6);
        piezas[0, 7] = new Torre(false, 0, 7);

        // Inicializar piezas blancas
        piezas[7, 0] = new Torre(true, 7, 0);
        piezas[7, 1] = new Caballo(true, 7, 1);
        piezas[7, 2] = new Alfil(true, 7, 2);
        piezas[7, 3] = new Reina(true, 7, 3);
        piezas[7, 4] = new Rey(true, 7, 4);
        piezas[7, 5] = new Alfil(true, 7, 5);
        piezas[7, 6] = new Caballo(true, 7, 6);
        piezas[7, 7] = new Torre(true, 7, 7);

        // Agregar piezas mayores a las listas
        piezasNegras.AddRange(new[] { piezas[0, 0], piezas[0, 1], piezas[0, 2], piezas[0, 3],
                                     piezas[0, 4], piezas[0, 5], piezas[0, 6], piezas[0, 7] });
        piezasBlancas.AddRange(new[] { piezas[7, 0], piezas[7, 1], piezas[7, 2], piezas[7, 3],
                                      piezas[7, 4], piezas[7, 5], piezas[7, 6], piezas[7, 7] });

        ActualizarTableroVisual();
    }

    private void ActualizarTableroVisual()
    {
        try
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // Restaurar color original
                    casillas[i, j].BackColor = (i + j) % 2 == 0 ? Color.White : Color.Gray;
                    
                    // Actualizar imagen de pieza
                    if (piezas[i, j] != null)
                    {
                        ActualizarImagenPieza(i, j);
                    }
                    else
                    {
                        if (casillas[i, j].BackgroundImage != null)
                        {
                            casillas[i, j].BackgroundImage.Dispose();
                            casillas[i, j].BackgroundImage = null;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error actualizando tablero visual: {ex.Message}");
        }
    }

//.............................................Imagenes(assets automaticos)
    private Image ObtenerImagenPieza(Pieza pieza)
    {
        Bitmap bmp = new Bitmap(50, 50);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush colorPieza = pieza.EsBlanco ? Brushes.White : Brushes.Black;
            Pen borde = new Pen(pieza.EsBlanco ? Color.Black : Color.White, 2);

            if (pieza is Peon)
                DibujarPeon(g, colorPieza, borde);
            else if (pieza is Torre)
                DibujarTorre(g, colorPieza, borde);
            else if (pieza is Caballo)
                DibujarCaballo(g, colorPieza, borde);
            else if (pieza is Alfil)
                DibujarAlfil(g, colorPieza, borde);
            else if (pieza is Reina)
                DibujarReina(g, colorPieza, borde);
            else if (pieza is Rey)
                DibujarRey(g, colorPieza, borde);
        }
        return bmp;
    }

    private void DibujarPeon(Graphics g, Brush color, Pen borde)
    {
        g.FillEllipse(color, 20, 10, 10, 10);
        g.DrawEllipse(borde, 20, 10, 10, 10);
        g.FillPolygon(color, new Point[] {
            new Point(15, 20),
            new Point(35, 20),
            new Point(25, 40)
        });
        g.DrawPolygon(borde, new Point[] {
            new Point(15, 20),
            new Point(35, 20),
            new Point(25, 40)
        });
    }

    private void DibujarTorre(Graphics g, Brush color, Pen borde)
    {
        g.FillRectangle(color, 15, 10, 20, 30);
        g.DrawRectangle(borde, 15, 10, 20, 30);
        g.FillRectangle(color, 10, 5, 30, 5);
        g.DrawRectangle(borde, 10, 5, 30, 5);
    }

    private void DibujarCaballo(Graphics g, Brush color, Pen borde)
    {
        Point[] puntos = {
            new Point(15, 40),
            new Point(15, 20),
            new Point(20, 15),
            new Point(25, 5),
            new Point(30, 15),
            new Point(35, 20),
            new Point(35, 40)
        };
        g.FillPolygon(color, puntos);
        g.DrawPolygon(borde, puntos);
    }

    private void DibujarAlfil(Graphics g, Brush color, Pen borde)
    {
        g.FillEllipse(color, 20, 5, 10, 10);
        g.DrawEllipse(borde, 20, 5, 10, 10);
        g.FillPolygon(color, new Point[] {
            new Point(15, 15),
            new Point(35, 15),
            new Point(25, 40)
        });
        g.DrawPolygon(borde, new Point[] {
            new Point(15, 15),
            new Point(35, 15),
            new Point(25, 40)
        });
    }

    private void DibujarReina(Graphics g, Brush color, Pen borde)
    {
        for (int i = 0; i < 4; i++)
        {
            g.FillEllipse(color, 10 + (i * 8), 5, 6, 6);
            g.DrawEllipse(borde, 10 + (i * 8), 5, 6, 6);
        }
        g.FillPolygon(color, new Point[] {
            new Point(10, 10),
            new Point(40, 10),
            new Point(25, 40)
        });
        g.DrawPolygon(borde, new Point[] {
            new Point(10, 10),
            new Point(40, 10),
            new Point(25, 40)
        });
    }

    private void DibujarRey(Graphics g, Brush color, Pen borde)
    {
        g.DrawLine(borde, 25, 2, 25, 8);
        g.DrawLine(borde, 22, 5, 28, 5);
        g.FillEllipse(color, 20, 10, 10, 10);
        g.DrawEllipse(borde, 20, 10, 10, 10);
        g.FillPolygon(color, new Point[] {
            new Point(15, 20),
            new Point(35, 20),
            new Point(25, 40)
        });
        g.DrawPolygon(borde, new Point[] {
            new Point(15, 20),
            new Point(35, 20),
            new Point(25, 40)
        });
    }

//.............................................Movimientos
    private bool movimientoRealizado = false;
    private void CasillaClick(int fila, int col)
    {
        try
        {
            if (piezaSeleccionada == null)
            {
                // Seleccionar pieza
                if (piezas[fila, col] != null && piezas[fila, col].EsBlanco == turnoBlancas)
                {
                    piezaSeleccionada = piezas[fila, col];
                    var movimientosValidos = piezaSeleccionada.MovimientosValidos(piezas);
                    if (movimientosValidos != null && movimientosValidos.Any())
                    {
                        ResaltarMovimientosValidos(piezaSeleccionada);
                    }
                }
            }
            else
            {
                if (EsMovimientoValido(piezaSeleccionada, fila, col))
                {
                    try
                    {
                        //..guardamos posición original
                        int filaOriginal = piezaSeleccionada.Fila;
                        int colOriginal = piezaSeleccionada.Columna;

                        //..se realiza el movimiento y guarda el resultado
                        movimientoRealizado = RealizarMovimiento(piezaSeleccionada, fila, col);

                        //..actualiza el tablero
                        piezas[fila, col] = piezaSeleccionada;
                        piezas[filaOriginal, colOriginal] = null;
                        
                        //..cambia el turno
                        turnoBlancas = !turnoBlancas;

                        //..actualiza visualmente
                        ActualizarTableroVisual();

                        if (!turnoBlancas)
                        {
                            Console.WriteLine("Iniciando turno de la IA..."); //..-debug
                            Application.DoEvents();
                            RealizarMovimientoIA();
                            turnoBlancas = true;
                            ActualizarTableroVisual();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error durante el movimiento: {ex.Message}");
                    }
                }

                LimpiarResaltado();
                piezaSeleccionada = null;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error en CasillaClick: {ex.Message}\n{ex.StackTrace}");
        }
    }


    private bool RealizarMovimiento(Pieza pieza, int fila, int col)
    {
        var movimientos = pieza.MovimientosValidos(piezas);
        if (movimientos.Any(m => m.X == fila && m.Y == col))
        {
            piezas[pieza.Fila, pieza.Columna] = null;
            pieza.Fila = fila;
            pieza.Columna = col;
            piezas[fila, col] = pieza;
            return true;
        }
        movimientoRealizado = true;
        return false;
    }

    private void RealizarMovimientoIA()
    {
        try 
        {
            //..genera todos los movimientos posibles para las piezas negras
            List<Movimiento> movimientosPosibles = new List<Movimiento>();
            
            //..recorre el tablero buscando piezas negras
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (piezas[i, j] != null && !piezas[i, j].EsBlanco)
                    {
                        var movimientos = piezas[i, j].MovimientosValidos(piezas);
                        foreach (var destino in movimientos)
                        {
                            movimientosPosibles.Add(new Movimiento(i, j, destino.X, destino.Y));
                        }
                    }
                }
            }

            if (movimientosPosibles.Count > 0)
            {
                //..evalua cada movimiento posible
                int mejorValor = int.MinValue;
                Movimiento mejorMovimiento = null;

                foreach (var movimiento in movimientosPosibles)
                {
                    //..crea una copia del tablero para simular el movimiento
                    var tableroTemporal = ClonarTablero(piezas);
                    
                    //..realiza el movimiento en el tablero temporal
                    var pieza = tableroTemporal[movimiento.FilaOrigen, movimiento.ColumnaOrigen];
                    tableroTemporal[movimiento.FilaDestino, movimiento.ColumnaDestino] = pieza;
                    tableroTemporal[movimiento.FilaOrigen, movimiento.ColumnaOrigen] = null;

                    //..evalua la posición resultante
                    int valorMovimiento = EvaluarTablero(tableroTemporal);

                    if (valorMovimiento > mejorValor)
                    {
                        mejorValor = valorMovimiento;
                        mejorMovimiento = movimiento;
                    }
                }

                //..así realizamos el mejor movimiento encontrado
                if (mejorMovimiento != null)
                {
                    var pieza = piezas[mejorMovimiento.FilaOrigen, mejorMovimiento.ColumnaOrigen];
                    piezas[mejorMovimiento.FilaDestino, mejorMovimiento.ColumnaDestino] = pieza;
                    piezas[mejorMovimiento.FilaOrigen, mejorMovimiento.ColumnaOrigen] = null;
                    
                    //..actualizamos la posición de la pieza
                    pieza.Fila = mejorMovimiento.FilaDestino;
                    pieza.Columna = mejorMovimiento.ColumnaDestino;

                    ActualizarTableroVisual();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error en movimiento IA: {ex.Message}");
        }
    }

    private void ActualizarTableroDesdeNodo(Nodo nodo)
    {
        if (nodo != null && nodo.Tablero != null)
        {
            //..copia estado del tablero del nodo al tablero actual
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    piezas[i, j] = nodo.Tablero[i, j];
                }
            }
            ActualizarTableroVisual();
        }
    }

    private static Pieza[,] RealizarMovimientoVirtual(Pieza[,] tablero, Movimiento movimiento)
    {
        Pieza[,] tableroTemporal = ClonarTablero(tablero);
        tableroTemporal[movimiento.FilaDestino, movimiento.ColumnaDestino] = tableroTemporal[movimiento.FilaOrigen, movimiento.ColumnaOrigen];
        tableroTemporal[movimiento.FilaOrigen, movimiento.ColumnaOrigen] = null;
        return tableroTemporal;
    }

    private void ResaltarMovimientosValidos(Pieza pieza)
    {
        var movimientos = pieza.MovimientosValidos(piezas);
        foreach (var movimiento in movimientos)
        {
            casillas[movimiento.X, movimiento.Y].BackColor = Color.LightGreen;
        }
    }

    private void LimpiarResaltado()
    {
        for (int fila = 0; fila < 8; fila++)
        {
            for (int col = 0; col < 8; col++)
            {
                casillas[fila, col].BackColor = (fila + col) % 2 == 0 ? Color.White : Color.Gray;
            }
        }
    }

    private bool EsMovimientoValido(Pieza pieza, int fila, int col)
    {
        var movimientos = pieza.MovimientosValidos(piezas);
        return movimientos.Any(m => m.X == fila && m.Y == col);
    }

    private void RealizarMovimiento(Pieza pieza, Point destino)
    {
        piezas[pieza.Fila, pieza.Columna] = null;
        pieza.Fila = destino.X;
        pieza.Columna = destino.Y;
        piezas[destino.X, destino.Y] = pieza;
    }

    private void ClonarTablero(Pieza[,] tableroOriginal, Pieza[,] tableroCopia)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tableroOriginal[i, j] != null)
                {
                    Type tipo = tableroOriginal[i, j].GetType();
                    tableroCopia[i, j] = (Pieza)Activator.CreateInstance(tipo, tableroOriginal[i, j].EsBlanco, i, j);
                }
            }
        }
    }

//.............................................Minimax
    private void Minimax(Nodo nodo, int profundidad, int alfa, int beta, bool esMaximizador)
    {
        if (profundidad == 0 || EsJaqueMate(nodo.Tablero))
        {
            nodo.Valor = EvaluarTablero(nodo.Tablero);
            return;
        }

        if (esMaximizador)
        {
            int mejorValor = int.MinValue;
            foreach (var movimiento in GenerarTodosMovimientos(nodo.Tablero, true))
            {
                var tableroTemporal = RealizarMovimientoVirtual(nodo.Tablero, movimiento);
                var nuevoNodo = new Nodo(tableroTemporal, nodo, profundidad - 1, false);
                nodo.Hijos.Add(nuevoNodo);
                Minimax(nuevoNodo, profundidad - 1, alfa, beta, false);
                mejorValor = Math.Max(mejorValor, nuevoNodo.Valor);
                alfa = Math.Max(alfa, mejorValor);
                if (beta <= alfa)
                {
                    break;
                }
            }
            nodo.Valor = mejorValor;
        }
        else
        {
            int mejorValor = int.MaxValue;
            foreach (var movimiento in GenerarTodosMovimientos(nodo.Tablero, false))
            {
                var tableroTemporal = RealizarMovimientoVirtual(nodo.Tablero, movimiento);
                var nuevoNodo = new Nodo(tableroTemporal, nodo, profundidad - 1, true);
                nodo.Hijos.Add(nuevoNodo);
                Minimax(nuevoNodo, profundidad - 1, alfa, beta, true);
                mejorValor = Math.Min(mejorValor, nuevoNodo.Valor);
                beta = Math.Min(beta, mejorValor);
                if (beta <= alfa)
                {
                    break;
                }
            }
            nodo.Valor = mejorValor;
        }
    }

    private void ActualizarImagenPieza(int fila, int col)
    {
        casillas[fila, col].BackgroundImage = ObtenerImagenPieza(piezas[fila, col]);
        casillas[fila, col].BackgroundImageLayout = ImageLayout.Stretch;
    }

    private void InicializarTablero()
    {
        for (int fila = 0; fila < 8; fila++)
        {
            for (int col = 0; col < 8; col++)
            {
                piezas[fila, col] = null;
            }
        }
    }

    private int EvaluarTablero(Pieza[,] tablero)
    {
        int valor = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tablero[i, j] != null)
                {
                    int valorPieza = ObtenerValorPieza(tablero[i, j]);
                    if (tablero[i, j].EsBlanco)
                        valor += valorPieza;
                    else
                        valor -= valorPieza;
                }
            }
        }
        return valor;
    }

    private int ObtenerValorPieza(Pieza pieza)
    {
        if (pieza is Peon) return 10;
        if (pieza is Caballo) return 30;
        if (pieza is Alfil) return 30;
        if (pieza is Torre) return 50;
        if (pieza is Reina) return 90;
        if (pieza is Rey) return 900;
        return 0;
    }

    private List<Movimiento> GenerarTodosMovimientos(Pieza[,] tablero, bool esBlanco)
    {
        List<Movimiento> movimientos = new List<Movimiento>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tablero[i, j] != null && tablero[i, j].EsBlanco == esBlanco)
                {
                    var movs = tablero[i, j].MovimientosValidos(tablero);
                    foreach (var mov in movs)
                    {
                        movimientos.Add(new Movimiento(i, j, mov.X, mov.Y));
                    }
                }
            }
        }
        return movimientos;
    }

    public class Movimiento
    {
        public int FilaOrigen { get; private set; }
        public int ColumnaOrigen { get; private set; }
        public int FilaDestino { get; private set; }
        public int ColumnaDestino { get; private set; }

        public Movimiento(int filaOrigen, int colOrigen, int filaDestino, int colDestino)
        {
            FilaOrigen = filaOrigen;
            ColumnaOrigen = colOrigen;
            FilaDestino = filaDestino;
            ColumnaDestino = colDestino;
        }
    }

//.............................................Clase Piezas y Nodo
    public abstract class Pieza
    {
        public bool EsBlanco { get; protected set; }
        public int Fila { get; set; }
        public int Columna { get; set; }

        protected Pieza(bool esBlanco, int fila, int columna)
        {
            EsBlanco = esBlanco;
            Fila = fila;
            Columna = columna;
        }

        public abstract List<Point> MovimientosValidos(Pieza[,] tablero);
        
        protected bool EstaEnTablero(int fila, int columna)
        {
            return fila >= 0 && fila < 8 && columna >= 0 && columna < 8;
        }

        protected bool PuedeMoverA(Pieza[,] tablero, int fila, int columna)
        {
            if (!EstaEnTablero(fila, columna)) return false;
            
            if (tablero[fila, columna] == null) return true;
            
            return tablero[fila, columna].EsBlanco != this.EsBlanco;
        }

        //..método para clonar la pieza (sirve para simular movimientos)
        public virtual Pieza Clonar()
        {
            //..crea una nueva instancia del mismo tipo que la pieza actual
            return (Pieza)Activator.CreateInstance(
                this.GetType(),  //..obtiene el tipo específico de la pieza (rey, alfil, etc.)
                new object[] { this.EsBlanco, this.Fila, this.Columna }
            );
        }
    }

    public class Peon : Pieza
    {
        public Peon(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            int direccion = EsBlanco ? -1 : 1;
            int filaInicial = EsBlanco ? 6 : 1;

            //..movimiento hacia adelante
            if (EstaEnTablero(Fila + direccion, Columna) && 
                tablero[Fila + direccion, Columna] == null)
            {
                movimientos.Add(new Point(Fila + direccion, Columna));

                //..movimiento doble desde posición inicial
                if (Fila == filaInicial && 
                    tablero[Fila + (2 * direccion), Columna] == null)
                {
                    movimientos.Add(new Point(Fila + (2 * direccion), Columna));
                }
            }

            //..capturas en diagonal
            int[] columnasCaptura = { Columna - 1, Columna + 1 };
            foreach (int col in columnasCaptura)
            {
                if (EstaEnTablero(Fila + direccion, col) && 
                    tablero[Fila + direccion, col] != null && 
                    tablero[Fila + direccion, col].EsBlanco != EsBlanco)
                {
                    movimientos.Add(new Point(Fila + direccion, col));
                }
            }

            return movimientos;
        }
    }

    public class Torre : Pieza
    {
        public Torre(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            int[] direcciones = { -1, 1 };

            //..movimientos verticales
            foreach (int dir in direcciones)
            {
                for (int f = Fila + dir; EstaEnTablero(f, Columna); f += dir)
                {
                    if (tablero[f, Columna] == null)
                    {
                        movimientos.Add(new Point(f, Columna));
                    }
                    else
                    {
                        if (tablero[f, Columna].EsBlanco != EsBlanco)
                        {
                            movimientos.Add(new Point(f, Columna));
                        }
                        break;
                    }
                }
            }

            //..movimientos horizontales
            foreach (int dir in direcciones)
            {
                for (int c = Columna + dir; EstaEnTablero(Fila, c); c += dir)
                {
                    if (tablero[Fila, c] == null)
                    {
                        movimientos.Add(new Point(Fila, c));
                    }
                    else
                    {
                        if (tablero[Fila, c].EsBlanco != EsBlanco)
                        {
                            movimientos.Add(new Point(Fila, c));
                        }
                        break;
                    }
                }
            }

            return movimientos;
        }
    }

    public class Caballo : Pieza
    {
        public Caballo(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            int[] movimientosFila = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] movimientosColumna = { -1, 1, -2, 2, -2, 2, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nuevaFila = Fila + movimientosFila[i];
                int nuevaColumna = Columna + movimientosColumna[i];

                if (EstaEnTablero(nuevaFila, nuevaColumna))
                {
                    if (tablero[nuevaFila, nuevaColumna] == null || 
                        tablero[nuevaFila, nuevaColumna].EsBlanco != EsBlanco)
                    {
                        movimientos.Add(new Point(nuevaFila, nuevaColumna));
                    }
                }
            }

            return movimientos;
        }
    }

    public class Alfil : Pieza
    {
        public Alfil(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            int[] direccionesFila = { -1, -1, 1, 1 };
            int[] direccionesColumna = { -1, 1, -1, 1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int f = Fila + direccionesFila[dir];
                int c = Columna + direccionesColumna[dir];

                while (EstaEnTablero(f, c))
                {
                    if (tablero[f, c] == null)
                    {
                        movimientos.Add(new Point(f, c));
                    }
                    else
                    {
                        if (tablero[f, c].EsBlanco != EsBlanco)
                        {
                            movimientos.Add(new Point(f, c));
                        }
                        break;
                    }
                    f += direccionesFila[dir];
                    c += direccionesColumna[dir];
                }
            }

            return movimientos;
        }
    }

    public class Reina : Pieza
    {
        public Reina(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            
            //..combina los movimientos de torre y alfil
            int[] direccionesFila = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] direccionesColumna = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int dir = 0; dir < 8; dir++)
            {
                int f = Fila + direccionesFila[dir];
                int c = Columna + direccionesColumna[dir];

                while (EstaEnTablero(f, c))
                {
                    if (tablero[f, c] == null)
                    {
                        movimientos.Add(new Point(f, c));
                    }
                    else
                    {
                        if (tablero[f, c].EsBlanco != EsBlanco)
                        {
                            movimientos.Add(new Point(f, c));
                        }
                        break;
                    }
                    f += direccionesFila[dir];
                    c += direccionesColumna[dir];
                }
            }

            return movimientos;
        }
    }

    public class Rey : Pieza
    {
        public Rey(bool esBlanco, int fila, int columna) : base(esBlanco, fila, columna) { }

        public override List<Point> MovimientosValidos(Pieza[,] tablero)
        {
            List<Point> movimientos = new List<Point>();
            int[] direccionesFila = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] direccionesColumna = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nuevaFila = Fila + direccionesFila[i];
                int nuevaColumna = Columna + direccionesColumna[i];

                if (EstaEnTablero(nuevaFila, nuevaColumna))
                {
                    if (tablero[nuevaFila, nuevaColumna] == null || 
                        tablero[nuevaFila, nuevaColumna].EsBlanco != EsBlanco)
                    {
                        if (!DejaReyEnJaque(tablero, nuevaFila, nuevaColumna))
                        {
                            movimientos.Add(new Point(nuevaFila, nuevaColumna));
                        }
                    }
                }
            }

            return movimientos;
        }

        private bool DejaReyEnJaque(Pieza[,] tablero, int nuevaFila, int nuevaColumna)
        {
            Pieza[,] tableroTemporal = ClonarTablero(tablero);
            tableroTemporal[nuevaFila, nuevaColumna] = tableroTemporal[Fila, Columna];
            tableroTemporal[Fila, Columna] = null;
            
            return EstaEnJaque(tableroTemporal, EsBlanco);
        }
    }

    public class Nodo
    {
        public Pieza[,] Tablero { get; private set; }

        public Nodo? Padre { get; set; }  //..se agrega '?' para permitir null
        public List<Nodo> Hijos { get; private set; }
        public int Profundidad { get; private set; }
        public bool EsMaximizador { get; private set; }
        public int Valor { get; set; }
        public Movimiento MovimientoRealizado { get; set; }

        public Nodo(Pieza[,] tablero, Nodo padre, int profundidad, bool esMaximizador)
        {
            Tablero = tablero;
            Padre = padre;  //..entonces puede ser null
            Profundidad = profundidad;
            EsMaximizador = esMaximizador;
            Hijos = new List<Nodo>();
            Valor = 0;
        }
    }

    private static Pieza[,] ClonarTablero(Pieza[,] tableroOriginal)
    {
        Pieza[,] clon = new Pieza[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tableroOriginal[i, j] != null)
                {
                    Type tipo = tableroOriginal[i, j].GetType();
                    clon[i, j] = (Pieza)Activator.CreateInstance(tipo, 
                        tableroOriginal[i, j].EsBlanco, i, j);
                }
            }
        }
        return clon;
    }

//.............................................Otros
    private Pieza[,] ClonarTablero()
    {
        Pieza[,] nuevoTablero = new Pieza[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (piezas[i, j] != null)
                {
                    //..crea una nueva instancia de la pieza
                    nuevoTablero[i, j] = piezas[i, j].Clonar();
                }
            }
        }
        return nuevoTablero;
    }

    private static bool EstaEnJaque(Pieza[,] tablero, bool esReyBlanco)
    {
        //..encuentra la posición del rey
        Point posicionRey = new Point(-1, -1);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tablero[i, j] is Rey && tablero[i, j].EsBlanco == esReyBlanco)
                {
                    posicionRey = new Point(i, j);
                    break;
                }
            }
        }

        //..verifica si alguna pieza enemiga puede atacar al rey
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tablero[i, j] != null && tablero[i, j].EsBlanco != esReyBlanco)
                {
                    var movimientos = tablero[i, j].MovimientosValidos(tablero);
                    if (movimientos.Any(m => m.X == posicionRey.X && m.Y == posicionRey.Y))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool EsJaqueMate(Pieza[,] tablero)
    {
        bool esBlancas = turnoBlancas;
        if (EstaEnJaque(tablero, esBlancas))
        {
            //..chequea si hay algún movimiento legal que evite el jaque
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (tablero[i, j] != null && tablero[i, j].EsBlanco == esBlancas)
                    {
                        var movimientos = tablero[i, j].MovimientosValidos(tablero);
                        foreach (var movimiento in movimientos)
                        {
                            var tableroTemporal = ClonarTablero(tablero);
                            tableroTemporal[movimiento.X, movimiento.Y] = tableroTemporal[i, j];
                            tableroTemporal[i, j] = null;
                            
                            if (!EstaEnJaque(tableroTemporal, esBlancas))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TableroAjedrez());
    }
}

//.............................................Fin del código
//@FECORO, Rengo. 2024
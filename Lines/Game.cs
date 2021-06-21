using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lines
{
	/// <summary>
	/// Структура для PictureBox, x - строка, y - столбец, color - цвета шарика
	/// </summary>
	public struct Ball
	{
		public int x;
		public int y;
		public int color;
	}
	class Game
	{
        #region общие переменные
        int max; // ширина и длина поля
		int[,] map; // 0 - пусто, 1-6 шарик цвета n;
		int max_colors = 6; // количество цветов шариков
		ShowItem Show; // делегат функции отображения
		Status status; // переменная для текущего статуса игры
		Ball[] ball = new Ball[3]; // массив из шариков для появления следующих
		static Random rand = new Random(); // рандом
        #endregion

        #region Переменные для выбора шариков
        Ball marketBall; // шарик который выбрали
		int marketJump; //переменная для анимации прыжка шарика
		Ball destinBall; // поле куда переместить
        #endregion

        #region Переменные для построения пути (Метод волны
        int[,] fmap; // Массив для метода волны
		Ball[] path; // Массив для клеток перемещения шарика
		int paths; // Переменная для счета
		int pathStep; // Шаг перемещения
        #endregion

        #region Переменные для уничтожения шариков
        int strips, stripStep; //
		Ball[] strip; // Шарики которые нужно убрать
        #endregion

        enum Status
		{
			init,//начало игры
			wait,// Ожидание выбора первого шарика
			ball_mark, // шарик выбран - отмечен, ожидаем выбора точки B
			path_show, // показать путь, передвижение шарика
			ball_move, // процесс передвижения шарика
			next_balls, // вывод подсказки по следующим шарикам
			line_strip, // "взрыв" собранных линий
			stop // поля заполнены, конец игры 
		}

		

		/// <summary>
		/// Конструктор для класса
		/// </summary>
		/// <param name="max"> длина поля</param>
		/// <param name="Show"> Делегат отображения поля</param>
		public Game(int max, ShowItem Show)
		{
			this.max = max;
			this.Show = Show;
			map = new int[max, max];
			fmap = new int[max, max];
			status = Status.init;
			path = new Ball[81];
			strip = new Ball[99];
		}
		
		/// <summary>
		/// Инициализация начальной карты
		/// </summary>
		private void InitMap()
        {
			Ball none;
			none.color = 0;
			for (int x = 0; x < max; x++)
				for (int y = 0; y < max; y++)
                {
					map[x, y] = 0;
					none.x = x;
					none.y = y;
					Show(none, Item.none);
                }

		}

		/// <summary>
		/// Прием параметров на нажатый PictureBox
		/// </summary>
		/// <param name="x">Координата X</param>
		/// <param name="y">Координата Y</param>
		public void ClickBox(int x, int y)
		{
			// Выбор шарика или выбор нового шарика
			if (status == Status.wait || status == Status.ball_mark)
			{
				if (map[x, y] > 0)
				{
					if (status == Status.ball_mark)
					{
						Show(marketBall, Item.ball);
					}
					marketBall.x = x;
					marketBall.y = y;
					marketBall.color = map[x, y];
					status = Status.ball_mark;
				}
			}

			// Выбор поля куда нужно переместить шарик
			if (status == Status.ball_mark)
			{
				if (map[x, y] <= 0)
				{
					destinBall.x = x;
					destinBall.y = y;
					destinBall.color = marketBall.color;
					if (FindPath())
						status = Status.path_show;
					return;
				}
			}
			// Перезапуск игры
			if(status == Status.stop)
				status = Status.init;
		}

		/// <summary>
		/// Функция состояний игры
		/// </summary>
		public void Step()
		{
			switch (status)
			{
				case Status.init: // Начало
					InitMap(); // Инициализация начальной карты
					SelectNextBalls(); // Создание 3 новых шариков-подсказок
					ShowNextBalls(); // Создание 3 шариков
					SelectNextBalls(); // Создание 3 новых шариков-подсказок
					status = Status.wait; // Переключение в режим ожидания
					break;
				case Status.wait: // В режиме ожидания ждем пока нажмут на один из шариков
					break;
				case Status.ball_mark: // Выбрали шарик
					JumpBall(); // Проигрываем анимацию прыжка
					break;
				case Status.path_show: // Построение пути
					PathShow(); // Построение и отображение пути
					break;
				case Status.ball_move: // Передвижение шарика
					MoveBall();
					break;
				case Status.next_balls: // Режим показа новых шариков подсказок
					ShowNextBalls(); // Создание 3 шариков
					SelectNextBalls(); // Создание 3 новых шариков-подсказок
					break;
				case Status.line_strip: // Проверка на собранные линии
					StripLines();
					break;
				case Status.stop: // Поле заполнено, конец игры
					break;
			}
		}

		/// <summary>
		/// Выбор новых Шариков-подсказок
		/// </summary>
		private void SelectNextBalls()
		{
			ball[0] = SelectNextBall();
			ball[1] = SelectNextBall();
			ball[2] = SelectNextBall();
		}

		/// <summary>
		/// Создание рандомных шариков-подсказок
		/// </summary>
		/// <returns></returns>
		private Ball SelectNextBall()
		{
			return SelectNextBall(rand.Next(1, max_colors + 1));
		}

		/// <summary>
		/// Создание шариков подсказок определенного цвета
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private Ball SelectNextBall(int color)
		{
			int loop = 100; // количество попыток поиска свободных полей
			Ball next;
			next.color = color;
			do
			{
				next.x = rand.Next(0, max);
				next.y = rand.Next(0, max);
				if (--loop < 0)
				{
					next.x = -1;
					return next;
				}
			} while (map[next.x, next.y] != 0);
			map[next.x, next.y] = -1;
			Show(next, Item.next);
			return next;
		}

		/// <summary>
		/// Поставить шарики на поле
		/// </summary>
		private void ShowNextBalls()
		{
			//Отобразить шарики из подсказок
			for (int i = 0; i < ball.Length; i++)
			{
				ShowNextBall(ball[i]);
			}
			//Проверка на собранные линии
			if (FindStripLines())
				status = Status.line_strip;
			else // Проверка на заполненное поле
				if (IsMapFull())
				status = Status.stop;
			else
				status = Status.wait;
		}

		/// <summary>
		/// Отображение Одного Шарика
		/// </summary>
		/// <param name="next"></param>
		private void ShowNextBall(Ball next)
		{
			if (next.x < 0) return;
			if (map[next.x, next.y] > 0)
			{
				next = SelectNextBall(next.color);
				if (next.x < 0) return;
			}
			map[next.x, next.y] = next.color;
			Show(next, Item.ball);
		}
		/// <summary>
		/// Отображение шариков-подсказок после пути или исчезновения
		/// </summary>
		private void HintNextBalls()
        {
			for (int i = 0; i < ball.Length; i++)
				HintNextBall(ball[i]);
        }

		private void HintNextBall(Ball next)
        {
			if (next.x < 0) return;
			Show(next, Item.next);
        }

		/// <summary>
		/// Проигрывание анимации выбранного шарика
		/// </summary>
		private void JumpBall()
		{
			if (status != Status.ball_mark)
				return;
			if (marketJump == 0)
				Show(marketBall, Item.jump);
			else
				Show(marketBall, Item.ball);
			marketJump = 1 - marketJump;
		}

		/// <summary>
		/// Пережвижение шарика
		/// </summary>
		private void MoveBall()
		{
			if (status != Status.ball_move)
				return;
			if (map[marketBall.x, marketBall.y] > 0 &&
				map[destinBall.x, destinBall.y] <= 0)
			{
				map[marketBall.x, marketBall.y] = 0;
				map[destinBall.x, destinBall.y] = marketBall.color;
				Show(marketBall, Item.none);
				Show(destinBall, Item.ball);
				if (FindStripLines())
					status = Status.line_strip;
				else
					status = Status.next_balls;
			}
		}

		/// <summary>
		/// Поиск пути (Модель волны)
		/// </summary>
		/// <returns></returns>
		private bool FindPath()
		{
			if (!(map[marketBall.x, marketBall.y] > 0 &&
				map[destinBall.x, destinBall.y] <= 0))
				return false;
			//Инициализация матрицы для пути
			for (int x = 0; x < max; x++)
				for (int y = 0; y < max; y++)
					fmap[x, y] = 0;
			bool added; // Переменная для добавления цифры
			bool found = false; // Переменная обнаружения пути
			int nr = 1; // Цифра которой заполняем матрицу
			fmap[marketBall.x, marketBall.y] = 1; // Ставим 1 на место где стоит шарик
			do
			{
				added = false;
				//Нахождение нужного числа
				for (int x = 0; x < max; x++)
					for (int y = 0; y < max; y++)
						if (fmap[x, y] == nr) // если нашли, то ставим вокруг следующую цифру
						{
							MarkPath(x + 1, y, nr + 1); // Проверяем справа
							MarkPath(x - 1, y, nr + 1); // Проверяем слева
							MarkPath(x, y + 1, nr + 1); // Проверяем сверху
							MarkPath(x, y - 1, nr + 1); // Проверяем снизу
							added = true;
						}
				if (fmap[destinBall.x, destinBall.y] > 0) // если поставили цифру в поле куда надо переместить шарик
				{
					found = true;
					break;
				}
				nr++;
			} while (added);

			if (!found) //Выходим если не нашли путь
				return false;
			//Переменные для каждой клетки перемещения
			int px = destinBall.x;
			int py = destinBall.y;

			paths = nr; // Цифра куда переместить
			//Идем с конечной точки
			while (nr >= 0)
			{
				path[nr].x = px;
				path[nr].y = py;
				//Ищем ближайшую цифру
				if (IsPath(px + 1, py, nr)) px++; else	// Смотрим справа
				if (IsPath(px - 1, py, nr)) px--; else	// Смотрим слева
				if (IsPath(px, py + 1, nr)) py++; else	// Смотрим сверху
				if (IsPath(px, py - 1, nr)) py--;		// Смотрим снизу
				nr--;
			}
			pathStep = 0; // Текущий шаг передвижения начальный
			return true;
		}

		/// <summary>
		/// Проверка на свободное поле, для выставления цифры для метода волны
		/// </summary>
		/// <param name="x">Координата x</param>
		/// <param name="y">Координата y</param>
		/// <param name="k">Цифра которую нужно поставить</param>
		private void MarkPath(int x, int y, int k)
		{
			if (x < 0 || x >= max) return; // Если вышли за строку
			if (y < 0 || y >= max) return; // Если вышли за столбик
			if (map[x, y] > 0) return; // Если уже стоит шарик
			if (fmap[x, y] > 0) return; // Если стоит цифра
			fmap[x, y] = k;
		}

		/// <summary>
		/// Функция проверки цифры пути
		/// </summary>
		/// <param name="x">Координата x</param>
		/// <param name="y">Координата y</param>
		/// <param name="k">Цифра которую нужно найти</param>
		/// <returns></returns>
		private bool IsPath(int x, int y, int k)
		{
			if (x < 0 || x >= max) return false;
			if (y < 0 || y >= max) return false;
			return (fmap[x, y] == k);
		}

		/// <summary>
		/// Функция отображения пути
		/// </summary>
		private void PathShow()
		{
			//Отображение клеток пути
			if (pathStep == 0)
			{
				for (int nr = 1; nr <= paths; nr++)
					Show(path[nr], Item.path);
				pathStep++;
				return;
			}

			Ball movingBall = new Ball(); // Передвижение шарика
			movingBall = path[pathStep - 1];
			Show(movingBall, Item.none); // Убираем шарик
			movingBall = path[pathStep];
			movingBall.color = marketBall.color;
			Show(movingBall, Item.ball); // Ставим на следующее поле
			pathStep++;

			if (pathStep > paths)
			{
				HintNextBalls();
				status = Status.ball_move;
			}
		}

		/// <summary>
		/// Нахождение собранных линий
		/// </summary>
		/// <returns></returns>
		private bool FindStripLines()
		{
			strips = 0;
			for (int x = 0; x < max; x++)
				for(int y = 0; y < max; y++)
				{
					CheckLine(x, y, 1, 0);	// cмотрим вправо
					CheckLine(x, y, 1, 1);	// смотрим вправовниз
					CheckLine(x, y, 0, 1);	// смотрим вниз
					CheckLine(x, y, -1, 1); // смотрим вниз влево
				}
			if (strips == 0)
				return false;
			stripStep = 4;
			return true;
		}

		/// <summary>
		/// Проверяем собранные линии и закидываем в массив шарики,
		/// которые нужно удалить
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		private void CheckLine(int x, int y, int sx, int sy)
		{
			int p = 4;
			if (x < 0 || x >= max) return;
			if (y < 0 || y >= max) return;
			if (x + p * sx < 0 || x + p * sx >= max) return;
			if (y + p * sy < 0 || y + p * sy >= max) return;
			int color = map[x, y];
			if (color <= 0) return;
			for (int k = 1; k <= p; k++)
				if (map[x + k * sx, y + k * sy] != color)
					return;
			for (int k = 0; k <= p; k++)
			{
				strip[strips].x = x + k * sx;
				strip[strips].y = y + k * sy;
				strip[strips].color = color;
				strips++;
			}
		}

		/// <summary>
		/// Удаление шариков
		/// </summary>
		private void StripLines()
		{
			if (stripStep <= 0)
			{
				for (int i = 0; i < strips; i++)
					map[strip[i].x, strip[i].y] = 0;
				HintNextBalls();
				status = Status.wait;
				return;
			}
			stripStep--;
			for (int i = 0; i < strips; i++)
			{
				switch (stripStep)
				{
					case 3: Show(strip[i], Item.jump); break;
					case 2: Show(strip[i], Item.ball); break;
					case 1: Show(strip[i], Item.next); break;
					case 0: Show(strip[i], Item.none); break;
				}
			}
		}

		/// <summary>
		/// Проверка на Заполненное полу
		/// </summary>
		/// <returns></returns>
		private bool IsMapFull()
		{
			for (int x = 0; x < max; x++)
				for (int y = 0; y < max; y++)
					if (map[x, y] <= 0)
						return false;
			return true;
		}
	}
}

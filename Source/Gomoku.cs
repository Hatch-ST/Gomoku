using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gomokunarabe{
	class Gomoku {
		//マスの状態
		enum TileState {
			Nothing,
			White,
			Black,
			Outside
		}
		//向き
		enum Direction {
			Up,
			Down,
			Left,
			Right,
			UpperLeft,
			UpperRight,
			LowerLeft,
			LowerRight
		}

		private const int MAP_SIZE = 15;			//盤面のサイズ
		private TileState[,] mapState;				//盤面の状態を保管する配列
		private int[] cursorPos = new int[2];		//カーソル位置
		private int[] myCood = new int[2];			//自分のいる座標

		private const int CURSOR_MAX_LEFT = 3;									//カーソル移動の最左端
		private const int CURSOR_MAX_TOP = 1;									//カーソル移動の最上端
		private const int CURSOR_MOVE_HORIZONTAL_SPEED = 2;						//カーソルの左右移動の幅
		private const int CURSOR_MOVE_VERTICAL_SPEED = 1;						//カーソルの上下移動の幅
		private const int CURSOR_MAX_WIDTH = 15 * CURSOR_MOVE_HORIZONTAL_SPEED;	//カーソル移動の左右幅
		private const int CURSOR_MAX_HEIGTH = 15 * CURSOR_MOVE_VERTICAL_SPEED;	//カーソル移動の上下幅

		//コンストラクタ
		public Gomoku() {
			//盤面を初期化する
			setMap();
		}

		int time = 1;
		//ゲームをスタートするメソッド
		public void Start() {
			int[] enemyCood = new int[2];
			TileState winner = TileState.Nothing;
			//盤面を表示する
			displayMap();
			//操作方法表示
			displayControllMethod();
			//カーソルを初期値に移動する
			cursorPos[0] = 3;
			cursorPos[1] = 1;
			Console.SetCursorPosition(CURSOR_MAX_LEFT, CURSOR_MAX_TOP);
			//ゲームのループ
			while (winner == TileState.Nothing) {
				//プレイヤーの入力
				while (waitInputKey()) ;
				//プレイヤーの勝利判定
				winner = judgeGame(myCood);
				if (winner != TileState.Nothing)
					break;
				Thread.Sleep(time);
				//敵の入力
				enemyCood = computerAI(TileState.White);
				putStone(enemyCood[0], enemyCood[1], TileState.White);
				//敵の勝利判定
				winner = judgeGame(enemyCood);
			}
			//勝者の表示
			Console.SetCursorPosition(0, CURSOR_MAX_TOP + CURSOR_MAX_HEIGTH + 2);
			displayWinner(winner);
			while (waitInputEnter()) ;
		}
		 
		//操作方法を表示するメソッド
		private void displayControllMethod() {
			Console.SetCursorPosition(0, CURSOR_MAX_TOP + CURSOR_MAX_HEIGTH + 2);
			Console.WriteLine("カーソルキー：移動");
			Console.WriteLine("Aキー：石を置く");
		}

		//勝者を表示するメソッド
		private void displayWinner(TileState winner) {
			string winnerName;
			if(winner == TileState.Black){
				winnerName = "黒";
			}else{
				winnerName = "白";
			}

			Console.WriteLine("{0}の勝ちです　　　",winnerName);
			Console.WriteLine("Enterを押すと終了します");
		}

		//キー操作を待つメソッド
		private bool waitInputKey() {
			ConsoleKeyInfo key = Console.ReadKey(true);
			switch (key.Key) {
				case ConsoleKey.UpArrow:
					moveCursor(0, -CURSOR_MOVE_VERTICAL_SPEED);
					break;
				case ConsoleKey.DownArrow:
					moveCursor(0, CURSOR_MOVE_VERTICAL_SPEED);
					break;
				case ConsoleKey.LeftArrow:
					moveCursor(-CURSOR_MOVE_HORIZONTAL_SPEED, 0);
					break;
				case ConsoleKey.RightArrow:
					moveCursor(CURSOR_MOVE_HORIZONTAL_SPEED, 0);
					break;
				case ConsoleKey.A:
					if (putStone(myCood[0], myCood[1], TileState.Black)) {
						return false;
					}
					break;
			}
			return true;
		}

		//エンターを押すまで待つメソッド
		private bool waitInputEnter(){
			ConsoleKeyInfo key = Console.ReadKey(true);
			switch(key.Key){
				case ConsoleKey.Enter:
					return false;
				default:
					return true;
			}
		}

		//カーソル位置を移動させるメソッド
		private void moveCursor(int moveX, int moveY) {
			cursorPos[0] += moveX;
			cursorPos[1] += moveY;

			//カーソルのはみ出しチェック
			if (cursorPos[0] < CURSOR_MAX_LEFT) {
				cursorPos[0] = CURSOR_MAX_LEFT;
			} else if (CURSOR_MAX_LEFT + CURSOR_MAX_WIDTH - 1 < cursorPos[0]) {
				cursorPos[0] = CURSOR_MAX_LEFT + CURSOR_MAX_WIDTH - 1;
			}
			if (cursorPos[1] < CURSOR_MAX_TOP) {
				cursorPos[1] = CURSOR_MAX_TOP;
			} else if (CURSOR_MAX_TOP + CURSOR_MAX_HEIGTH - 1 < cursorPos[1]) {
				cursorPos[1] = CURSOR_MAX_TOP + CURSOR_MAX_HEIGTH - 1;
			}

			//カーソル位置から座標を取得
			myCood[0] = (cursorPos[0] - CURSOR_MAX_LEFT) / CURSOR_MOVE_HORIZONTAL_SPEED;
			myCood[1] = (cursorPos[1] - CURSOR_MAX_TOP) / CURSOR_MOVE_VERTICAL_SPEED;

			Console.SetCursorPosition(cursorPos[0], cursorPos[1]);
		}

		//碁石を置くメソッド
		private bool putStone(int x, int y, TileState tileState) {
			if (mapState[x, y] == TileState.Nothing) {
				mapState[x, y] = tileState;
				drawStone(x, y);
				return true;
			}
			return false;
		}

		//置いた碁石を表示するメソッド
		private void drawStone(int x, int y) {
			int[] drawCood = new int[2];
			drawCood[0] = x * CURSOR_MOVE_HORIZONTAL_SPEED + CURSOR_MAX_LEFT;
			drawCood[1] = y * CURSOR_MOVE_VERTICAL_SPEED + CURSOR_MAX_TOP;
			Console.SetCursorPosition(drawCood[0], drawCood[1]);
			drawMark(x, y);
			Console.SetCursorPosition(cursorPos[0], cursorPos[1]);
		}

		//盤面を初期化するメソッド
		private void setMap() {
			mapState = new TileState[MAP_SIZE, MAP_SIZE];
		}

		//盤面を表示するメソッド
		private void displayMap() {
			//盤面を表示する
			for (int y = -1; y < MAP_SIZE; y++) {

				for (int x = -1; x < MAP_SIZE; x++) {
					//１行目にx座標を表示する
					if (y == -1) {
						if (x == -1) {
							Console.Write("   ");
						} else {
							Console.Write("{0,2}", x + 1);
						}
					}
						//１列目にy座標を表示する
					else if (x == -1) {
						Console.Write("{0,2} ", y + 1);
					} else {
						drawMark(x, y);
					}
					//行の最後に改行する
					if (x == MAP_SIZE - 1) {
						Console.WriteLine();
					}
				}
			}
		}

		//碁石の記号を表示するメソッド
		private void drawMark(int x, int y) {
			//盤面が白の場合
			if (mapState[x, y] == TileState.White) {
				Console.Write("○");
			}
				//盤面が黒の場合
			else if (mapState[x, y] == TileState.Black) {
				Console.Write("●");
			}
				//盤面がなしの場合
			else {
				Console.Write("┼");
			}
		}

		//勝敗が決定したか判定するメソッド
		private TileState judgeGame(int[] pos) {
			TileState stoneColor = mapState[pos[0], pos[1]];
			//横の個数をチェックする
			int horizontalNum = 1;
			horizontalNum += GetColorNum(pos, stoneColor, Direction.Right, -1);
			horizontalNum += GetColorNum(pos, stoneColor, Direction.Left, -1);
			if (horizontalNum >= 5)
				return stoneColor;
			//縦の個数をチェックする
			int verticalNum = 1;
			verticalNum += GetColorNum(pos, stoneColor, Direction.Up, -1);
			verticalNum += GetColorNum(pos, stoneColor, Direction.Down, -1);
			if (verticalNum >= 5)
				return stoneColor;
			//右斜めの個数をチェックする
			int diagonallyRightNum = 1;
			diagonallyRightNum += GetColorNum(pos, stoneColor, Direction.UpperRight, -1);
			diagonallyRightNum += GetColorNum(pos, stoneColor, Direction.LowerLeft, -1);
			if (diagonallyRightNum >= 5)
				return stoneColor;
			//左斜めの個数をチェックする
			int diagonallyLeftNum = 1;
			diagonallyLeftNum += GetColorNum(pos, stoneColor, Direction.UpperLeft, -1);
			diagonallyLeftNum += GetColorNum(pos, stoneColor, Direction.LowerRight, -1);
			if (diagonallyLeftNum >= 5)
				return stoneColor;

			return TileState.Nothing;
		}

		//盤面の座標から同じ色の碁石が何個並んでいるかを調べるメソッド
		private int GetColorNum(int[] pos, TileState color, Direction dir, int stoneNum) {
			//盤面の端の場合、再起呼び出し終了
			if (pos[0] < 0 || MAP_SIZE - 1 < pos[0] || pos[1] < 0 || MAP_SIZE - 1 < pos[1])
				return stoneNum;
			//受け取った座標が違う碁石の場合、再起呼び出し終了
			if (mapState[pos[0], pos[1]] != color) {
				return stoneNum;
			}

			stoneNum++;
			//座標を移動する
			int[] nextPos = new int[2];
			for (int i = 0; i < 2; i++) {
				nextPos[i] = pos[i];
			}
			//向きによる分岐
			switch (dir) {
				case Direction.Up:
					nextPos[1]--;
					break;
				case Direction.Down:
					nextPos[1]++;
					break;
				case Direction.Left:
					nextPos[0]--;
					break;
				case Direction.Right:
					nextPos[0]++;
					break;
				case Direction.UpperLeft:
					nextPos[1]--;
					nextPos[0]--;
					break;
				case Direction.UpperRight:
					nextPos[1]--;
					nextPos[0]++;
					break;
				case Direction.LowerLeft:
					nextPos[1]++;
					nextPos[0]--;
					break;
				case Direction.LowerRight:
					nextPos[1]++;
					nextPos[0]++;
					break;
			}
			//メソッドの再起呼び出し
			stoneNum = GetColorNum(nextPos, color, dir, stoneNum);
			return stoneNum;
		}


		private int[] computerAI(TileState myColor) {
			int[] pos = new int[2];
			int[,] mapPriority = new int[MAP_SIZE, MAP_SIZE];
			//各マスの優先度を調べる
			for (int y = 0; y < MAP_SIZE; y++) {
				for (int x = 0; x < MAP_SIZE; x++) {
					mapPriority[x, y] = checkAround(x, y, myColor);
				}
			}

			int maxPriority = 0;
			//優先度の高いものを選ぶ
			Random random = new Random();
			for (int y = 0; y < MAP_SIZE; y++) {
				for (int x = 0; x < MAP_SIZE; x++) {
					//優先度の高いものを選択
					if (maxPriority < mapPriority[x, y]) {
						maxPriority = mapPriority[x, y];
						pos[0] = x;
						pos[1] = y;
					}
					//優先度が同じ場合は1/2で置き換え
					else if (maxPriority == mapPriority[x, y]) {
						
						int num;
						if ((num = random.Next(2)) == 0) {
							maxPriority = mapPriority[x, y];
							pos[0] = x;
							pos[1] = y;

						}
					}
				}
			}
			return pos;
		}

		//その座標の周りを調べるメソッド
		private int checkAround(int x, int y,TileState myColor) {
			int priorityNum = 0;
			int[] pos = new int[2];
			pos[0] = x;
			pos[1] = y;
			//その座標の石が置かれている場合
			if (mapState[pos[0], pos[1]] != TileState.Nothing) {
				return -1;
			}
			//左チェック
			pos[0]--;
			priorityNum += getPriorityNum(pos, myColor, Direction.Left);

			//左上チェック
			pos[1]--;
			priorityNum += getPriorityNum(pos, myColor, Direction.UpperLeft);

			//上チェック
			pos[0]++;
			priorityNum += getPriorityNum(pos, myColor, Direction.Up);

			//右上チェック
			pos[0]++;
			priorityNum += getPriorityNum(pos, myColor, Direction.UpperRight);

			//右チェック
			pos[1]++;
			priorityNum += getPriorityNum(pos, myColor, Direction.Right);

			//右下チェック
			pos[1]++;
			priorityNum += getPriorityNum(pos, myColor, Direction.LowerRight);

			//下チェック
			pos[0]--;
			priorityNum += getPriorityNum(pos, myColor, Direction.Down);

			//左下チェック
			pos[0]--;
			priorityNum += getPriorityNum(pos, myColor, Direction.LowerLeft);

			//元の位置に戻す
			pos[0]++;
			pos[1]--;

			return priorityNum;
		}

		//優先度を返すメソッド
		private int getPriorityNum(int[] pos, TileState myColor,Direction direction) {
			int priorityNum = 0;
			//はみ出しチェック
			if (pos[0] < 0 || MAP_SIZE - 1 < pos[0] || pos[1] < 0 || MAP_SIZE - 1 < pos[1])
				return -1;
			if (mapState[pos[0], pos[1]] == TileState.Nothing) {
				return priorityNum;
			} 
			//個数をチェックする
			int stoneNum = 1;
			stoneNum += GetColorNum(pos, (TileState)(mapState[pos[0], pos[1]]), direction, -1);
			priorityNum += getPriorityPoint(stoneNum,pos,myColor,direction);
			
			return priorityNum;
		}

		//碁石の個数によって優先度の数値を返すメソッド
		private int getPriorityPoint(int stoneNum,int[] pos,TileState myColor,Direction direction) {
			//反対側の石を確認する
			TileState oppositionStone = checkOppositionStone(stoneNum, pos, direction);
			//相手の碁石が置かれている場合
			if (mapState[pos[0], pos[1]] != myColor) {
				switch (stoneNum) {
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 3;
					case 3:
						if (oppositionStone == myColor) {
							return 5;
						} else if (oppositionStone == TileState.Nothing) {
							return 20;
						} else {
							return 4;
						}
					case 4:
						if (oppositionStone == myColor) {
							return 30;
						} else if (oppositionStone == TileState.Nothing) {
							return 0;
						} else {
							return 30;
						}
					default:
						return -1;
				}
			} 
			//自分の碁石の場合
			else {
				switch (stoneNum) {
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 2;
					case 3:
						if (oppositionStone == TileState.Nothing) {
							return 10;
						}else if (oppositionStone != myColor) {
							return 4;
						} else {
							return 3;
						}
					case 4:
						return 100;
					default:
						return -1;
				}
			}
		}

		//並んでいる碁石の反対側に置かれている碁石を調べるメソッド
		private TileState checkOppositionStone(int stoneNum,int[] pos, Direction direction) {
			int[] nextPos = new int[2];
			for (int i = 0; i < 2; i++) {
				nextPos[i] = pos[i];
			}
			//向きによる分岐
			switch (direction) {
				case Direction.Up:
					nextPos[1] -= stoneNum;
					break;
				case Direction.Down:
					nextPos[1] += stoneNum;
					break;
				case Direction.Left:
					nextPos[0] -= stoneNum;
					break;
				case Direction.Right:
					nextPos[0] += stoneNum;
					break;
				case Direction.UpperLeft:
					nextPos[1] -= stoneNum;
					nextPos[0] -= stoneNum;
					break;
				case Direction.UpperRight:
					nextPos[1] -= stoneNum;
					nextPos[0] += stoneNum;
					break;
				case Direction.LowerLeft:
					nextPos[1] += stoneNum;
					nextPos[0] -= stoneNum;
					break;
				case Direction.LowerRight:
					nextPos[1] += stoneNum;
					nextPos[0] += stoneNum;
					break;
			}
			//はみ出しチェック
			if (nextPos[0] < 0 || MAP_SIZE - 1 < nextPos[0] || nextPos[1] < 0 || MAP_SIZE - 1 < nextPos[1])
				return TileState.Outside;
			//反対側の石を渡す
			return mapState[nextPos[0], nextPos[1]];
		}

	}
}

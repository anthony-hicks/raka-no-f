#include <Windows.h>
#include <tchar.h>
#include <iostream>

#define HOTKEY1 1000
#define HOTKEY2 1002
#define MY_TRAY_ICON_MESSAGE 0x8001

HINSTANCE g_hInst = NULL;

static TCHAR szWindowClass[] = _T("raka-no-f");
static TCHAR szTitle[] = _T("Raka No F");

BOOL InitApplication(HINSTANCE hInstance);
BOOL InitWindowHandle(HINSTANCE hInstance, HWND& hWnd, int nCmdShow);
BOOL InitTrayIcon(const HWND& hWnd, NOTIFYICONDATA& nid);
BOOL RegisterHotKeys(HWND hWnd);
BOOL CleanUp(NOTIFYICONDATA& nid);

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE, LPSTR lpCmdLine, int nCmdShow)
{
	MSG msg;
	HWND hWnd = NULL;
	NOTIFYICONDATA nid = {};

	if (!InitApplication(hInstance))
	{
		// TODO: Properly log an error message for windows?
		MessageBox(NULL, _T("Failed to init application!"), NULL, NULL);
		return 1;
	}

	if (!InitWindowHandle(hInstance, hWnd, nCmdShow))
	{
		MessageBox(NULL, _T("Failed to create window!"), NULL, NULL);
		return 1;
	}

	if (!InitTrayIcon(hWnd, nid))
	{
		MessageBox(NULL, _T("Failed to init tray icon!"), NULL, NULL);
		return 1;
	}

	if (!RegisterHotKeys(hWnd))
	{
		MessageBox(NULL, _T("Failed to register hotkeys!"), NULL, NULL);
		return 1;
	}

	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	if (!CleanUp(nid))
	{
		MessageBox(NULL, _T("Failed to cleanup!"), NULL, NULL);
		return 1;
	}

	return (int)msg.wParam;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	PAINTSTRUCT ps;
	HDC hdc;
	TCHAR greeting[] = _T("Hello, World");
	COLORREF white = RGB(255, 255, 255);
	COLORREF green = RGB(125, 255, 255);

	switch (message)
	{
	case WM_PAINT:
		hdc = BeginPaint(hWnd, &ps);
		SetBkMode(hdc, TRANSPARENT);
		SetTextColor(hdc, green);
		TextOut(hdc, 5, 5, greeting, _tcslen(greeting));
		TextOut(hdc, 5, 35, greeting, _tcslen(greeting));
		EndPaint(hWnd, &ps);
		break;
	case WM_SIZE:
		if (wParam == SIZE_MINIMIZED)
		{
			ShowWindow(hWnd, SW_HIDE);
		}
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	case WM_HOTKEY:
	//case WM_COMMAND:
		switch (LOWORD(wParam))
		{
		case HOTKEY1:
			OutputDebugString(_T("Hotkey1 was pressed.\n"));
			break;
		case HOTKEY2:
			OutputDebugString(_T("Hotkey2 was pressed.\n"));
			break;
		default:
			OutputDebugString(_T("Unknown hotkey was pressed.\n"));
			break;
		}
		break;
	case MY_TRAY_ICON_MESSAGE:
		switch (lParam)
		{
		case WM_LBUTTONDBLCLK:
			ShowWindow(hWnd, SW_RESTORE);
			SetForegroundWindow(hWnd);
			break;
		case WM_RBUTTONDOWN:
			OutputDebugString(_T("You right clicked the tray icon.\n"));
			break;
			//case WM_CONTEXTMENU: ShowContextMenu(hWnd);
		}
	//case WM_COMMAND:
	//	switch (LOWORD(wParam))
	//	{
	//	case MY_MENU_MSG1:
	//		OutputDebugString(_T("MENU MESSAGE 1\n"));
	//		break;-
	//	}
	//	break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
		break;
	}
	return 0;
}

BOOL InitApplication(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_APPLICATION));
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_APPLICATION));

	return RegisterClassEx(&wcex);
}

BOOL InitWindowHandle(HINSTANCE hInstance, HWND& hWnd, int nCmdShow)
{
	g_hInst = hInstance; // Store instance handle in global var. Idk why, but all the tutorials have this.

	INT startX = CW_USEDEFAULT;
	INT startY = CW_USEDEFAULT;
	INT width = 500;
	INT height = 100;

	//DWORD windowStyle = WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TRANSPARENT;
	DWORD windowStyle = WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOPMOST;
	//DWORD windowOptions = WS_POPUP;
	DWORD windowOptions = NULL;

	hWnd = CreateWindowEx(
		windowStyle,
		szWindowClass,		 // name of window class
		szTitle,			 // title-bar string
		windowOptions,
		startX,
		startY,
		width,
		height,
		(HWND) NULL,		 // owner window
		(HMENU) NULL,		 // menu handle/child-window identifier
		hInstance,           // handle to application instance
		(LPVOID) NULL        // window-creation data
	);

	if (!hWnd)
	{
		return FALSE;
	}

	/*HRGN GGG = CreateRectRgn(startX, startY, width, height);
	InvertRgn(GetDC(hWnd), GGG);
	SetWindowRgn(hWnd, GGG, false);

	COLORREF RRR = RGB(255, 0, 255);
	SetLayeredWindowAttributes(hWnd, RRR, (BYTE)0, LWA_COLORKEY);*/
	// 1. Create a solid background color
	// 2. Set that color as the window's transparent color
	//    Anything on the window that is not using that color will not be transparent. The OS will handle the rest for you.
	// Should use color keying (LWA_COLORKEY)
	SetLayeredWindowAttributes(hWnd, 0, 100, LWA_ALPHA);

	ShowWindow(hWnd, nCmdShow);
	//ShowWindow(hWnd, SW_HIDE);
	UpdateWindow(hWnd);

	//DeleteObject(GGG);
	return TRUE;
}

BOOL InitTrayIcon(const HWND& hWnd, NOTIFYICONDATA& nid)
{
	nid.uVersion = NOTIFYICON_VERSION_4;
	nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	nid.uID = 0x123456;
	nid.cbSize = sizeof(NOTIFYICONDATAA);
	nid.uTimeout = 10000;
	nid.hWnd = hWnd;
	nid.uCallbackMessage = MY_TRAY_ICON_MESSAGE;
	wcsncpy_s(nid.szInfoTitle, 64, _T("Title for balloon"), 64);
	wcsncpy_s(nid.szInfo, 256, _T("Body for balloon"), 256);
	// Do NOT set the NIF_INFO flag. idk why

	// Add the icon
	if (!Shell_NotifyIcon(NIM_ADD, &nid))
		return FALSE;

	if (!Shell_NotifyIcon(NIM_SETVERSION, &nid))
		return FALSE;

	return TRUE;
}

BOOL RegisterHotKeys(HWND hWnd)
{
	if (!RegisterHotKey(hWnd, HOTKEY1, MOD_ALT + MOD_SHIFT, 0x53)) // ALT+SHIFT+s
		return FALSE;
	if (!RegisterHotKey(hWnd, HOTKEY2, MOD_ALT + MOD_SHIFT, 0x51)) // ALT+SHIFT+q
		return FALSE;

	return TRUE;
}

BOOL CleanUp(NOTIFYICONDATA& nid)
{
	if (!Shell_NotifyIcon(NIM_DELETE, &nid))
		return FALSE;
}
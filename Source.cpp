#include <Windows.h>
#include <tchar.h>
#include <iostream>

#define HOTKEY1 1000
#define HOTKEY2 1002
#define MY_TRAY_ICON_MESSAGE 0x8001

static TCHAR szWindowClass[] = _T("win32app");

static TCHAR szTitle[] = _T("Win32 Guided Tour Application");

HINSTANCE hInst;

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
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
	wcex.hbrBackground = (HBRUSH) (COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_APPLICATION));

	if (!RegisterClassEx(&wcex))
	{
		MessageBox(NULL, L"Call to RegisterClassEx failed!", L"Win32 Guided Tour", NULL);
		return 1;
	}

	hInst = hInstance; // Store instance handle in global var

	HWND hWnd = CreateWindow(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 500, 100, NULL, NULL, hInstance, NULL);

	if (!hWnd)
	{
		MessageBox(NULL, _T("Call to CreateWindow failed!"), _T("Win32 Guided Tour"), NULL);
		return 1;
	}

	// TODO: function to populate
	NOTIFYICONDATA nid = {};
	nid.uVersion = NOTIFYICON_VERSION_4;
	nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	nid.uID = 0x123456;
	nid.cbSize = sizeof(NOTIFYICONDATAA);
	nid.uTimeout = 10000;
	nid.hWnd = hWnd;
	nid.uCallbackMessage = MY_TRAY_ICON_MESSAGE;
	wcsncpy_s(nid.szInfoTitle, 64, L"Title for balloon", 64);
	wcsncpy_s(nid.szInfo, 256, L"Body for balloon", 256);

	// Do NOT set the NIF_INFO flag.
	Shell_NotifyIcon(NIM_ADD, &nid); // Add the icon
	Shell_NotifyIcon(NIM_SETVERSION, &nid);

	// Pass HWND for the app level hotkey, not global.
	if (RegisterHotKey(hWnd, HOTKEY1, MOD_ALT + MOD_SHIFT, 0x53))
	{
		_tprintf(_T("Hokey 'ALT+SHIFT+s' registered.\n"));
	}
	else
	{
		_tprintf(_T("failed to register hokey.\n"));
		return 1;
	}

	RegisterHotKey(hWnd, HOTKEY2, MOD_ALT + MOD_SHIFT, 0x51); // ALT+SHIFT+q

	// Not using nCmdShow means you're not respecting how the caller wishes your app to appear (or not) at startup.
	// We're going to want to have a window, but only in-game, initially.
	//ShowWindow(hWnd, nCmdShow);
	ShowWindow(hWnd, SW_HIDE);
	UpdateWindow(hWnd);

	MSG msg; 
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	Shell_NotifyIcon(NIM_DELETE, &nid);

	return (int)msg.wParam;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	PAINTSTRUCT ps;
	HDC hdc;
	TCHAR greeting[] = _T("Hello, World");
	//MessageBox(NULL, _T("hotkey1 was pressed"), NULL, NULL);

	switch (message)
	{
	/*case WM_PAINT:
		hdc = BeginPaint(hWnd, &ps);
		TextOut(hdc, 5, 5, greeting, _tcslen(greeting));
		EndPaint(hWnd, &ps);
		break;*/
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
			MessageBox(NULL, L"Hotkey1 was pressed.", NULL, NULL);
			break;
		case HOTKEY2:
			MessageBox(NULL, L"Hotkey2 was pressed.", NULL, NULL);
			break;
		default:
			MessageBox(NULL, L"Unknown hotkey was pressed.", NULL, NULL);
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
			MessageBox(NULL, L"You right clicked", NULL, NULL);
			break;
			//case WM_CONTEXTMENU: ShowContextMenu(hWnd);
		}
	//case WM_COMMAND:
	//	switch (LOWORD(wParam))
	//	{
	//	case MY_MENU_MSG1:
	//		MessageBox(NULL, L"MENU MESSAGE 1", NULL, NULL);
	//		break;-
	//	}
	//	break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
		break;
	}
	return 0;
}
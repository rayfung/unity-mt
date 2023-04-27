#include <X11/Xlib.h>
#include <X11/extensions/XInput2.h>
#include <X11/Xutil.h>

#define MT_UNKNOWN (-1)
#define MT_NONE 0
#define MT_BEGIN 1
#define MT_UPDATE 2
#define MT_END 3

typedef struct
{
	Display *display;
	int xi_opcode;
}mt_context;

typedef struct
{
	int type;
	int id;
	float x;
	float y;
}mt_event;

void *mt_initialize();
mt_event mt_get_next(void *ctx);
void mt_finalize(void *ctx);

#include <stdio.h>
#include <unistd.h>
#include <time.h>
#include <assert.h>
#include <signal.h>

#include "libmt.h"

volatile int g_running = 1;

void handle_sigint(int signum)
{
	g_running = 0;
}

long get_time_ms()
{
	struct timespec ts;
	int ret = clock_gettime(CLOCK_REALTIME, &ts);
	assert(ret == 0);
	return ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

int main(int argc, char *argv[])
{
	signal(SIGINT, handle_sigint);

	mt_context *ctx = (mt_context *)mt_initialize();
	if (!ctx)
	{
		return 1;
	}

	int frame_number = 0;
	long max_ms = 0;

	while (g_running)
	{
		int show = 0;
		frame_number++;

		long begin_ms;
		begin_ms = get_time_ms();
		while (1)
		{
			mt_event event = mt_get_next(ctx);

			if (event.type != MT_NONE && !show)
			{
				printf("##### Frame %5d #####\n", frame_number);
				show = 1;
			}

			switch (event.type)
			{
				case MT_BEGIN:
					printf("Begin : %-6d (%.1f, %.1f)\n", event.id, event.x, event.y);
					break;

				case MT_UPDATE:
					printf("Update: %-6d (%.1f, %.1f)\n", event.id, event.x, event.y);
					break;

				case MT_END:
					printf("End   : %-6d (%.1f, %.1f)\n", event.id, event.x, event.y);
					break;

				case MT_NONE:
					goto frame_end;
			}
		}

frame_end:
		if (show)
		{
			long cur_ms = get_time_ms() - begin_ms;
			max_ms = cur_ms > max_ms ? cur_ms : max_ms;
			printf("Time  : %ld ms\n", cur_ms);
			printf("Max   : %ld ms\n", max_ms);
			printf("#######################\n");
		}

		usleep(16000);
	}

	printf("\nExiting...\n");
	mt_finalize(ctx);
	return 0;
}

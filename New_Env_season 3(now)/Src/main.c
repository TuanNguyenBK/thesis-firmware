/* Includes ------------------------------------------------------------------*/
#include "stm32f4xx_hal.h"



/* Private variables ---------------------------------------------------------*/
ADC_HandleTypeDef hadc1;

I2C_HandleTypeDef hi2c1;
DMA_HandleTypeDef hdma_i2c1_rx;

TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim4;

UART_HandleTypeDef huart2;
UART_HandleTypeDef huart3;
DMA_HandleTypeDef hdma_usart2_rx;
DMA_HandleTypeDef hdma_usart3_rx;


/* Private variables ---------------------------------------------------------*/
uint8_t send_data=32,receive_data[5],echo_receive_data[1],count=0;
char* data="backhoa123\r";
uint16_t duty0=0,duty1=0,duty2=0,duty3=0,C=0,D=0,Output=0,Output1=0,adc_value = 0;;
uint32_t pre_Pulse=0,Pulse=0,pulse=0,p=500,TocDoDat=0,Sampling_time=200,inv_Sampling_time=5;
uint32_t pre_Pulse1=0,Pulse1=0,pulse1=0,p1=500,TocDoDat1=0,time_copy=0,time=0;
uint32_t pre_Pulse2=0,Pulse2=0,pulse2=0,p2=500,TocDoDat2=0;
float Distance,Show = 0,Kp,Ki,Kd;
signed long des_Speed=400,rSpeed=0,Err=0,pre_Err=0,pre_pre_Err=0;
signed long rSpeed1=0,Err1=0,pre_Err1=0,pre_pre_Err1=0;
signed long rSpeed2=0,Err2=0,pre_Err2=0,pre_pre_Err2=0;
int start=0,sample_count=0,au=0,motor1=0,motor2=0,motor3=0;


/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_DMA_Init(void);
static void MX_TIM4_Init(void);
static void MX_USART2_UART_Init(void);
static void MX_USART3_UART_Init(void);
static void MX_I2C1_Init(void);
static void MX_ADC1_Init(void);
static void MX_TIM2_Init(void);

void HAL_TIM_MspPostInit(TIM_HandleTypeDef *htim);
    
void Motorpid(void);	
void Motorpid_1(void);	
void Duty_Config(void);
void Sonic(void);
void Go_straight (void);
void Go_back (void);
void Go_left (void);
void Go_right (void);
void Stop (void);
/* Private function prototypes -----------------------------------------------*/
void delay_us(uint16_t period){

   __HAL_RCC_TIM6_CLK_ENABLE();
     TIM6->PSC = 83;      // clk = SystemCoreClock /2/(PSC+1) = 1MHz
     TIM6->ARR = period-1;
     TIM6->CNT = 0;
     TIM6->EGR = 1;      // update registers;

     TIM6->SR  = 0;      // clear overflow flag
     TIM6->CR1 = 1;      // enable Timer6

     while (!TIM6->SR);
    
     TIM6->CR1 = 0;      // stop Timer6
     //RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM6, DISABLE);
   __HAL_RCC_TIM6_CLK_DISABLE();
}
   

int main(void)
{


  HAL_Init();

  /* Configure the system clock */
  SystemClock_Config();

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_DMA_Init();
  MX_TIM4_Init();
  MX_USART2_UART_Init();
  MX_USART3_UART_Init();
  MX_I2C1_Init();
  MX_ADC1_Init();
  MX_TIM2_Init();

	
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_1);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_2);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_3);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_4);
	 HAL_TIM_Base_Start_IT(&htim2);
	 HAL_UART_Receive_DMA(&huart2,receive_data,5);
	
  while (1)
  {
	//TocDoDat=100;
	des_Speed=300;
	Kp=0.5;
	Ki=0.1;
	Kd=0;

//		if(sample_count>=50)
//		{count++;
//		sample_count=0;
//		des_Speed=des_Speed+100;
//		}
		if(receive_data[0]=='S')
				{
					start=1;
						if(receive_data[1]=='L')duty3=50;					
						if(receive_data[1]=='M')duty3=150;					
						if(receive_data[1]=='H')duty3=350;
					
						if(receive_data[4]=='a')au=1;
						if(receive_data[4]=='m')au=0;
					 Output=0;Output1=0;
				}		
		if(receive_data[0]=='D')
				{
					start=0;
					Stop();
					duty3=0;					
				}
					
		if (start==1)
			{
			//Sonic();	
				if(au==1)
				{
						if(HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_0) == 1 ||HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_1) == 0||HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_7) == 0|| Show<3)
										 {
												Go_back();								 
										 }
						 else 
										 {
												Go_left();		
												Go_straight();
										 }		
				}
				
				if(au==0)
				{	
						if(receive_data[0]=='t')Go_left();		//trai
						if(receive_data[0]=='p')Go_right();		//phai
						if(receive_data[0]=='h')Go_straight();//di thang		
						if(receive_data[0]=='d')Stop();				//dung lai
						if(receive_data[0]=='l')Go_back();		//di lui
				}
			}
		}
}


void Go_straight (void)
{
	motor1=1;motor2=1;
}
void Go_back (void)
{
	motor3=1;
}
void Go_left (void)
{
	motor1=1;
}
void Go_right (void)
{ 
	motor2=1;
}
void Stop(void)
{
	motor1=0,motor2=0,motor3=0;	
	duty0=0;duty1=0;duty2=0;	
	Output=0;Output1=0;
	
}

void HAL_ADC_ConvCpltCallback(ADC_HandleTypeDef* hadc)
{
	if(hadc->Instance == hadc1.Instance)
	{
			adc_value = HAL_ADC_GetValue(hadc);
	}
}
void Motorpid_1(void)
{

rSpeed=(Pulse-pre_Pulse)*300/400; //tinh van toc (trong sampling time)
pre_Pulse=Pulse;
Err=des_Speed-rSpeed;
Output = Output+Kp*Err+Ki*Sampling_time*(Err+pre_Err)/(2000)+Kd*(Err-2*pre_Err+pre_pre_Err)*inv_Sampling_time;
if (Output >700) Output=400;
if (Output <=0) Output=0;
if(motor1==1)duty1=Output;
if (motor3==1)duty0=Output;
//Duty_Config();
pre_pre_Err= pre_Err;
pre_Err=Err; 
}

void Motorpid_2(void)
{
rSpeed1=(Pulse1-pre_Pulse1)*300/400; //tinh van toc (trong sampling time)
pre_Pulse1=Pulse1;
Err1=des_Speed-rSpeed1;
Output1 = Output1+Kp*Err1+Ki*Sampling_time*(Err1+pre_Err1)/(2000)+Kd*(Err1-2*pre_Err1+pre_pre_Err1)*inv_Sampling_time;
if (Output1 >700) Output1=400;
if (Output1 <=0) Output1=0;
if(motor2==1)duty2=Output1;	
//Duty_Config();
pre_pre_Err1= pre_Err1;
pre_Err1=Err1; 
}



void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance==htim2.Instance)
	{
//		if(motor3==1)
//		{
//			C=300*pulse/400;
//			pulse=0;	Motorpid_1();	
//		}		
//		if(motor1==1)
//		{
//			C=300*pulse/400;
//			pulse=0;	Motorpid_1();	
//		}		
//		if(motor2==1)
//		{	
//			D=300*pulse1/400;
//			pulse1=0;Motorpid_2();
//		}
		C=300*pulse/400;
		D=300*pulse1/400;
		Motorpid_1();	
		Motorpid_2();
		pulse=0;pulse1=0;
		Duty_Config();
		//sample_count++;
		//Duty_Config();
		
	}
}

void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
		if(GPIO_Pin==GPIO_PIN_5)
		{
				Pulse++;
				pulse++;
					//while(HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_0));
		}
		if(GPIO_Pin==GPIO_PIN_4)
		{
				Pulse1++;
				pulse1++;
					//   HAL_GPIO_TogglePin(GPIOD,GPIO_PIN_13);
				 //while(HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_4));
		}

}


void Duty_Config(void)
{			__HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_1,duty0);		//motor driver
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_2,duty1);		//motor driver
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_3,duty2);		//vacuum cleaner
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_4,duty3);
}


void Sonic(void)
{
 HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, GPIO_PIN_RESET);
     delay_us(2); //2 micro seconds
       
      //then we create a pulse for 10us
         HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, GPIO_PIN_SET);
         delay_us(10); 
         HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, GPIO_PIN_RESET);
         
         time=0;

      while (HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_8) == 0);
      while (HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_8))
      {
         time++;
         delay_us(2); // delay 2 us
         time_copy=time;
         Distance=(float)(time_copy*0.0346);
      }
			  HAL_Delay(50);
}


void SystemClock_Config(void)
{

  RCC_OscInitTypeDef RCC_OscInitStruct;
  RCC_ClkInitTypeDef RCC_ClkInitStruct;

  __HAL_RCC_PWR_CLK_ENABLE();

  __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE1);

  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSE;
  RCC_OscInitStruct.HSEState = RCC_HSE_ON;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSE;
  RCC_OscInitStruct.PLL.PLLM = 8;
  RCC_OscInitStruct.PLL.PLLN = 336;
  RCC_OscInitStruct.PLL.PLLP = RCC_PLLP_DIV2;
  RCC_OscInitStruct.PLL.PLLQ = 4;
  HAL_RCC_OscConfig(&RCC_OscInitStruct);

  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV4;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV4;
  HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_5);

  HAL_SYSTICK_Config(HAL_RCC_GetHCLKFreq()/1000);

  HAL_SYSTICK_CLKSourceConfig(SYSTICK_CLKSOURCE_HCLK);

  /* SysTick_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(SysTick_IRQn, 0, 0);
}

/* ADC1 init function */
void MX_ADC1_Init(void)
{

  ADC_ChannelConfTypeDef sConfig;

    /**Configure the global features of the ADC (Clock, Resolution, Data Alignment and number of conversion) 
    */
  hadc1.Instance = ADC1;
  hadc1.Init.ClockPrescaler = ADC_CLOCK_SYNC_PCLK_DIV2;
  hadc1.Init.Resolution = ADC_RESOLUTION_12B;
  hadc1.Init.ScanConvMode = DISABLE;
  hadc1.Init.ContinuousConvMode = ENABLE;
  hadc1.Init.DiscontinuousConvMode = DISABLE;
  hadc1.Init.ExternalTrigConvEdge = ADC_EXTERNALTRIGCONVEDGE_NONE;
  hadc1.Init.DataAlign = ADC_DATAALIGN_RIGHT;
  hadc1.Init.NbrOfConversion = 1;
  hadc1.Init.DMAContinuousRequests = DISABLE;
  hadc1.Init.EOCSelection = ADC_EOC_SINGLE_CONV;
  HAL_ADC_Init(&hadc1);

    /**Configure for the selected ADC regular channel its corresponding rank in the sequencer and its sample time. 
    */
  sConfig.Channel = ADC_CHANNEL_1;
  sConfig.Rank = 1;
  sConfig.SamplingTime = ADC_SAMPLETIME_144CYCLES;
  HAL_ADC_ConfigChannel(&hadc1, &sConfig);

}

/* I2C1 init function */
void MX_I2C1_Init(void)
{

  hi2c1.Instance = I2C1;
  hi2c1.Init.ClockSpeed = 400000;
  hi2c1.Init.DutyCycle = I2C_DUTYCYCLE_2;
  hi2c1.Init.OwnAddress1 = 0;
  hi2c1.Init.AddressingMode = I2C_ADDRESSINGMODE_7BIT;
  hi2c1.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
  hi2c1.Init.OwnAddress2 = 0;
  hi2c1.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
  hi2c1.Init.NoStretchMode = I2C_NOSTRETCH_DISABLE;
  HAL_I2C_Init(&hi2c1);

}

/* TIM2 init function */
void MX_TIM2_Init(void)
{

  TIM_ClockConfigTypeDef sClockSourceConfig;
  TIM_MasterConfigTypeDef sMasterConfig;

  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 42000;
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 399;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  HAL_TIM_Base_Init(&htim2);

  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  HAL_TIM_ConfigClockSource(&htim2, &sClockSourceConfig);

  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig);

}

/* TIM4 init function */
void MX_TIM4_Init(void)
{

  TIM_ClockConfigTypeDef sClockSourceConfig;
  TIM_MasterConfigTypeDef sMasterConfig;
  TIM_OC_InitTypeDef sConfigOC;

  htim4.Instance = TIM4;
  htim4.Init.Prescaler = 21;
  htim4.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim4.Init.Period = 400;
  htim4.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  HAL_TIM_Base_Init(&htim4);

  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  HAL_TIM_ConfigClockSource(&htim4, &sClockSourceConfig);

  HAL_TIM_PWM_Init(&htim4);

  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  HAL_TIMEx_MasterConfigSynchronization(&htim4, &sMasterConfig);

  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_1);

  HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_2);

  HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_3);
	
	//HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_4);
	HAL_TIM_MspPostInit(&htim4);

}

/* USART2 init function */
void MX_USART2_UART_Init(void)
{

  huart2.Instance = USART2;
  huart2.Init.BaudRate = 115200;
  huart2.Init.WordLength = UART_WORDLENGTH_8B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_NONE;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_16;
  HAL_UART_Init(&huart2);

}

/* USART3 init function */
void MX_USART3_UART_Init(void)
{

  huart3.Instance = USART3;
  huart3.Init.BaudRate = 115200;
  huart3.Init.WordLength = UART_WORDLENGTH_8B;
  huart3.Init.StopBits = UART_STOPBITS_1;
  huart3.Init.Parity = UART_PARITY_NONE;
  huart3.Init.Mode = UART_MODE_TX_RX;
  huart3.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart3.Init.OverSampling = UART_OVERSAMPLING_16;
  HAL_UART_Init(&huart3);

}

/** 
  * Enable DMA controller clock
  */
void MX_DMA_Init(void) 
{
  /* DMA controller clock enable */
  __HAL_RCC_DMA1_CLK_ENABLE();

  /* DMA interrupt init */
  /* DMA1_Stream0_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(DMA1_Stream0_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(DMA1_Stream0_IRQn);
  /* DMA1_Stream1_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(DMA1_Stream1_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(DMA1_Stream1_IRQn);
  /* DMA1_Stream5_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(DMA1_Stream5_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(DMA1_Stream5_IRQn);

}

/** Configure pins as 
        * Analog 
        * Input 
        * Output
        * EVENT_OUT
        * EXTI
*/
void MX_GPIO_Init(void)
{

  GPIO_InitTypeDef GPIO_InitStruct;

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOH_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOB_CLK_ENABLE();
  __HAL_RCC_GPIOD_CLK_ENABLE();

  /*Configure GPIO pin : PA0 */
  GPIO_InitStruct.Pin = GPIO_PIN_0;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA4 PA5 PA6 */
  GPIO_InitStruct.Pin = GPIO_PIN_4|GPIO_PIN_5|GPIO_PIN_6;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA7 PA8 */
  GPIO_InitStruct.Pin = GPIO_PIN_7|GPIO_PIN_8;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PB0 PB1 */
  GPIO_InitStruct.Pin = GPIO_PIN_0|GPIO_PIN_1;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pin : PD15 */
//  GPIO_InitStruct.Pin = GPIO_PIN_15;
//  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
//  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
//  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
//  HAL_GPIO_Init(GPIOD, &GPIO_InitStruct);

  /*Configure GPIO pin : PA10 */
  GPIO_InitStruct.Pin = GPIO_PIN_10;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOD, GPIO_PIN_15, GPIO_PIN_RESET);

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, GPIO_PIN_RESET);

  /* EXTI interrupt init*/
  HAL_NVIC_SetPriority(EXTI0_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(EXTI0_IRQn);

  HAL_NVIC_SetPriority(EXTI4_IRQn, 1, 0);
  HAL_NVIC_EnableIRQ(EXTI4_IRQn);

  HAL_NVIC_SetPriority(EXTI9_5_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);

}

/* USER CODE BEGIN 4 */

/* USER CODE END 4 */

#ifdef USE_FULL_ASSERT

/**
   * @brief Reports the name of the source file and the source line number
   * where the assert_param error has occurred.
   * @param file: pointer to the source file name
   * @param line: assert_param error line source number
   * @retval None
   */
void assert_failed(uint8_t* file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
    ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */

}

#endif

/**
  * @}
  */ 

/**
  * @}
*/ 

/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/

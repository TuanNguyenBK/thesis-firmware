/* Includes ------------------------------------------------------------------*/
#include "stm32f4xx_hal.h"

/* Private variables ---------------------------------------------------------*/
ADC_HandleTypeDef hadc1;

I2C_HandleTypeDef hi2c1;
DMA_HandleTypeDef hdma_i2c1_rx;

TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim3;
TIM_HandleTypeDef htim4;

UART_HandleTypeDef huart2;
DMA_HandleTypeDef hdma_usart2_rx;

/* Private variables ---------------------------------------------------------*/
uint8_t send_data[20], send_data_temp[20],i=0,receive_data[11],echo_receive_data[1],count=0;
char* data="92345\r";
uint16_t duty0=0,duty1=0,duty2=0,duty3=0,duty4=0,tmp_duty4=0,duty5=0,tmp_duty5=0,C=0,D=0,Output=0,Output1=0;
uint16_t Output2=0,Output3=0;
uint16_t start_hour=0,start_minute=0,time_out=0;
uint32_t pre_Pulse=0,Pulse=0,pulse=0,p=0,TocDoDat=0,Sampling_time=20,inv_Sampling_time=50;
uint32_t pre_Pulse1=0,Pulse1=0,pulse1=0,p1=0,TocDoDat1=0,time_copy=0,time=0;
uint16_t count_time_delay=0,count_time_ADC=0;

float Distance,Show = 0,Kp,Ki,Kd,Kp2,Ki2,Kd2;
float Kp_Pos,Ki_Pos,Kd_Pos;
signed long des_Speed=400,des_Position=700;
signed long	rSpeed=0,Err=0,pre_Err=0,pre_pre_Err=0;
signed long rSpeed1=0,Err1=0,pre_Err1=0,pre_pre_Err1=0;
//Position
signed long Err2=0,pre_Err2=0,pre_pre_Err2=0,pros2=0,loop=0;
signed long Err3=0,pre_Err3=0,pre_pre_Err3=0,pros3=0,loop1=0;

int start=0,run=0,sample_count=0,au=0,motor_left=0,motor_right=0,motor_straight=0,motor_back=0;
int	Deg_90_left=0,Deg_90_right=0,left=0,right=0,a=0,stop1=1,stop2=0,report=0,b=0,cont=0;
int stuck=0;
uint16_t stop_hour=0,stop_minute=0;
uint16_t adc_value,adc;

	//RTC DS3231
#define DS3231_ADD (0x68<<1)
#define DS3231_REG_TIME (0X00)
#define DS3231_REG_ALARM1 (0x07)
#define DS3231_REG_ALARM2 (0x0B)
#define DS3231_REG_CTRL (0x0E)
#define DS3231_REG_STATUS (0x0F)
#define DS3231_REG_TEMPERATURE (0x11)

typedef struct{
uint8_t sec;
uint8_t min;
uint8_t hour;
uint8_t day;
uint8_t date;
uint8_t month;
uint8_t year;

uint8_t I2C_Buffer[8];
}DS3231_t;
DS3231_t DS3231;

	//Compass MQC5883
#define QMC5883_ADDR             		 (0x0D<<1)
#define QMC5883_REG_STATUS           (0x06)
#define QMC5883_REG_CONFIG_1         (0x09)
#define QMC5883_REG_CONFIG_2         (0x0A)
#define QMC5883_REG_IDENT_B          (0x0B)

typedef struct{
int16_t x;
int16_t y;
int16_t z;


uint8_t I2C_Buffer_Write[2];
uint8_t I2C_Buffer_Read[6];
}QMC5883_t;
QMC5883_t QMC5883;

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_DMA_Init(void);
static void MX_TIM4_Init(void);
static void MX_USART2_UART_Init(void);
static void MX_I2C1_Init(void);
static void MX_ADC1_Init(void);
static void MX_TIM2_Init(void);
static void MX_TIM3_Init(void);

void HAL_TIM_MspPostInit(TIM_HandleTypeDef *htim);

void Motorpid_2(void);	
void Motorpid_1(void);
void Motor_Left_Position_PID(void);
void Motor_Right_Position_PID(void);
void Duty_Config(void);
void Sonic(void);
void Go_Straight (void);
void Go_Back (void);
void Go_Left (void);
void Go_Right (void);
void Stop (void);
void Send_Data(void);
void Analyze_RecieveArray(void);
void Zigziag_Mode(void);
int StuckMachine(void);
void Get_Battery(void);

void HAL_TIM_MspPostInit(TIM_HandleTypeDef *htim);
uint8_t RTC_BCD2DEC(uint8_t e);
uint8_t RTC_DEC2BCD(uint8_t e);
void DEC_ASCII_3(uint16_t adc, uint8_t send_data_temp[20], uint8_t i);
void DEC_ASCII_2(uint8_t num, uint8_t send_data_temp[20], uint8_t i);
void I2C_WriteBuffer(I2C_HandleTypeDef hi, uint8_t DEV_ADDR,uint8_t pDat[], uint8_t sizebuf);
void I2C_ReadBuffer(I2C_HandleTypeDef hi, uint8_t DEV_ADDR, uint8_t pData[], uint8_t sizebuf);
void RTC_GetTime(void);
void RTC_SetTime(uint8_t hour,uint8_t min,uint8_t sec,uint8_t day,uint8_t date,uint8_t month,uint8_t year);
void QMC5883_GetData(void);
void QMC5883_Config(void);

void DEC_ASCII_3(uint16_t adc, uint8_t send_data_temp[20], uint8_t i)
{
	send_data_temp[i]=adc/100+48;
	send_data_temp[i+1]=(adc%100)/10+48;
	send_data_temp[i+2]=adc%10+48;
}

void DEC_ASCII_2(uint8_t num, uint8_t send_data_temp[20], uint8_t i)
{
	send_data_temp[i]=num/10+48;
	send_data_temp[i+1]=num%10+48;
}
uint8_t RTC_BCD2DEC(uint8_t e)
{
	return (e>>4)*10 + (e&0x0F);
}

uint8_t RTC_DEC2BCD(uint8_t e)
{
	return (e/10)<<4 | (e%10);
}

void I2C_WriteBuffer(I2C_HandleTypeDef hi, uint8_t DEV_ADDR, uint8_t pData[], uint8_t sizebuf)
{
	while (HAL_I2C_Master_Transmit(&hi, (uint16_t) DEV_ADDR, (uint8_t*)pData, (uint16_t) sizebuf, (uint32_t)1000))
		{
			if(HAL_I2C_GetError(&hi) != HAL_I2C_ERROR_AF)
				{
					printf("Write Buffer Error\r\n");
				}
		}
}

void I2C_ReadBuffer(I2C_HandleTypeDef hi, uint8_t DEV_ADDR, uint8_t pData[], uint8_t sizebuf)
{
	while(HAL_I2C_Master_Receive(&hi, (uint16_t) DEV_ADDR, (uint8_t*)pData, (uint16_t) sizebuf, (uint32_t)1000))
		{
			if(HAL_I2C_GetError(&hi) != HAL_I2C_ERROR_AF)
				{
					printf("Read Buffer Error\r\n");
				}
		}
}

void RTC_GetTime(void)
{
	//Bat dau qua trinh nhan du lieu tu thanh ghi 0x00
	DS3231.I2C_Buffer[0]=0x00;
	I2C_WriteBuffer(hi2c1,(uint16_t) DS3231_ADD,DS3231.I2C_Buffer,1);
	while (HAL_I2C_GetState(&hi2c1) != HAL_I2C_STATE_READY);
	I2C_ReadBuffer(hi2c1,(uint16_t) DS3231_ADD,DS3231.I2C_Buffer,7);
	
	DS3231.sec = RTC_BCD2DEC(DS3231.I2C_Buffer[0]);
	DS3231.min = RTC_BCD2DEC(DS3231.I2C_Buffer[1]);
	DS3231.hour = RTC_BCD2DEC(DS3231.I2C_Buffer[2]);
	DS3231.day = RTC_BCD2DEC(DS3231.I2C_Buffer[3]);
	DS3231.date = RTC_BCD2DEC(DS3231.I2C_Buffer[4]);
	DS3231.month = RTC_BCD2DEC(DS3231.I2C_Buffer[5]);
	DS3231.year = RTC_BCD2DEC(DS3231.I2C_Buffer[6]);
}

void RTC_SetTime(uint8_t hour,uint8_t min,uint8_t sec,uint8_t day,uint8_t date,uint8_t month,uint8_t year)
{
	DS3231.I2C_Buffer[0] = 0x00;
	DS3231.I2C_Buffer[1] = RTC_DEC2BCD(sec);
	DS3231.I2C_Buffer[2] = RTC_DEC2BCD(min);
	DS3231.I2C_Buffer[3] = RTC_DEC2BCD(hour);
	DS3231.I2C_Buffer[4] = RTC_DEC2BCD(day);
	DS3231.I2C_Buffer[5] = RTC_DEC2BCD(date);
	DS3231.I2C_Buffer[6] = RTC_DEC2BCD(month);
	DS3231.I2C_Buffer[7] = RTC_DEC2BCD(year);
	
	I2C_WriteBuffer(hi2c1,(uint16_t) DS3231_ADD,DS3231.I2C_Buffer,8);
	HAL_Delay(200);
}
void QMC5883_GetData(void)
{
		//Bat dau qua trinh nhan du lieu tu thanh ghi 0x00
	QMC5883.I2C_Buffer_Write[0]=0x00;
	I2C_WriteBuffer(hi2c1,(uint16_t) QMC5883_ADDR,QMC5883.I2C_Buffer_Write,1);
	while (HAL_I2C_GetState(&hi2c1) != HAL_I2C_STATE_READY);

	HAL_Delay(200);
	I2C_ReadBuffer(hi2c1,(uint16_t) QMC5883_ADDR,QMC5883.I2C_Buffer_Read,6);
	
	QMC5883.x = (QMC5883.I2C_Buffer_Read[1]<<8)+QMC5883.I2C_Buffer_Read[0];
	QMC5883.y = (QMC5883.I2C_Buffer_Read[3]<<8)+QMC5883.I2C_Buffer_Read[2];
	QMC5883.z = (QMC5883.I2C_Buffer_Read[5]<<8)+QMC5883.I2C_Buffer_Read[4];

}

void QMC5883_Config(void)
{
	QMC5883.I2C_Buffer_Write[0] = 0x0B;
	QMC5883.I2C_Buffer_Write[1] = 0x01;
	I2C_WriteBuffer(hi2c1,(uint16_t) QMC5883_ADDR,QMC5883.I2C_Buffer_Write,2);
	HAL_Delay(200);
	QMC5883.I2C_Buffer_Write[0] = 0x09;
	QMC5883.I2C_Buffer_Write[1] = 0x95;
	I2C_WriteBuffer(hi2c1,(uint16_t) QMC5883_ADDR,QMC5883.I2C_Buffer_Write,2);
}

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
void delay_01ms(uint16_t period){

   __HAL_RCC_TIM6_CLK_ENABLE();
     //RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM6, ENABLE);
     TIM6->PSC = 8399;      // clk = SystemCoreClock /2 /(PSC+1) = 10KHz
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
/* Private function prototypes -----------------------------------------------*/

int main(void)
{

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* Configure the system clock */
  SystemClock_Config();

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_DMA_Init();
  MX_TIM4_Init();
  MX_USART2_UART_Init();
  MX_I2C1_Init();
  MX_ADC1_Init();
  MX_TIM2_Init();
  MX_TIM3_Init();


	 HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_1);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_2);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_3);
   HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_4);
	 HAL_TIM_PWM_Start(&htim3,TIM_CHANNEL_1);
	 HAL_TIM_PWM_Start(&htim3,TIM_CHANNEL_2);
	 HAL_TIM_Base_Start_IT(&htim2);
	 HAL_UART_Receive_DMA(&huart2,receive_data,11);
		//QMC5883_Config();

  while (1)
  {
		//QMC5883_GetData();
		RTC_GetTime();	
		Get_Battery();
		Analyze_RecieveArray();
		Send_Data();
		
		des_Speed=30;					//set up setpoint PID
		Kp=0.9;Ki=0.6;Kd=0.0002;	
		Kp2=0.05;Ki2=0.5;Kd2=0.0001;	
		des_Position=730;		
		Kp_Pos=0.3;Ki_Pos=0.08;Kd_Pos=0;
		Zigziag_Mode();
		if (start==1)
			{				
				if(au==1&&run==1)
				{			
						Zigziag_Mode();
				}
				
				if(au==0)
				{		duty5= tmp_duty5;duty4= tmp_duty4;
						if(receive_data[0]=='t')Go_Left();		//trai
						if(receive_data[0]=='p')Go_Right();		//phai
						if(receive_data[0]=='h')Go_Straight();//di thang		
						if(receive_data[0]=='d')Stop();				//dung lai
						if(receive_data[0]=='l')Go_Back();		//di lui
				}
			}


  }
}

int StuckMachine(){
		if((Output>200||Output1>200||Output2>200||Output3>200)&&(C==0&&D==0))
		{stuck=1;}
		else stuck=0;
		return stuck;
}

void Get_Battery(void){
			if(count_time_ADC>50){
			count_time_ADC=0;
			HAL_ADC_Start_IT(&hadc1);
			HAL_Delay(50);
			HAL_ADC_Stop_IT(&hadc1);}
}

void Analyze_RecieveArray(void)
{
		if(receive_data[0]=='S')
				{
						start=1;report=0;tmp_duty4=90;
						if(receive_data[1]=='L')tmp_duty5=250;					
						if(receive_data[1]=='M')tmp_duty5=325;					
						if(receive_data[1]=='H')tmp_duty5=400;					
						if(receive_data[8]=='a')au=1;
						if(receive_data[8]=='m')au=0;
						  time_out=(receive_data[2]-48)*10+(receive_data[3]-48);
							start_hour=(receive_data[4]-48)*10+(receive_data[5]-48);
							start_minute=(receive_data[6]-48)*10+(receive_data[7]-48);
							stop_hour=(start_hour*60+start_minute+time_out)/60;
							stop_minute=(start_hour*60+start_minute+time_out)%60;
				}		
		if(receive_data[0]=='D')
				{
					start=0;stop1=1;stop2=0;
					report=1;Deg_90_left=0;Deg_90_right=0;
					Stop();run=0;a=0;left=0;right=0;
					duty5=0;tmp_duty5=0;
					duty4=0;tmp_duty4=0;						
				}
		send_data[16]='0';
		if(au==1)
		{
					if((DS3231.hour==start_hour&&DS3231.min==start_minute)||(start_hour==0&&start_minute==0))
					{run=1;duty5=tmp_duty5;duty4=tmp_duty4;}
					else if(DS3231.hour>=stop_hour&&DS3231.min>=stop_minute&&time_out!=0&&run==1)
					{run=0;Stop();duty5=0;duty4=0;send_data[16]='s';}							
		}
}


void Send_Data(void)
{
		//send value
		DEC_ASCII_3(adc_value*100/4096,send_data,0); //ADC value (battery)
		DEC_ASCII_3(100-adc_value*100/4096,send_data,3); //COMPLETE VALUE
		//send time
		DEC_ASCII_2(DS3231.sec,send_data,6);		//second
		DEC_ASCII_2(DS3231.min,send_data,8);		//minutes
		DEC_ASCII_2(DS3231.hour,send_data,10);		//hours
		DEC_ASCII_2(DS3231.date,send_data,12);		//day
		DEC_ASCII_2(DS3231.month,send_data,14);		//month
		send_data[17]= '\r';
		HAL_UART_Transmit_IT(&huart2,(uint8_t  *)send_data,18);
	
}


void Zigziag_Mode(void)
{						Sonic();//||HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_1) == 0||HAL_GPIO_ReadPin(GPIOD,GPIO_PIN_9) == 0
						//if(HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_0) == 0 || StuckMachine()==1 )
						if(Distance< 4 || StuckMachine()==1 )
									{
										if(stop1==1)
											 {Stop();HAL_Delay(500);
												stop1=0;
												 	if(a==0)		//* co quay trai, phai cua xe
														{left=1;right=0;//a=1-a;
														}
													else	
														{left=0,right=1;//a=1-a;
														}	
												}
										else if (Deg_90_left!=1&&Deg_90_right!=1)
										{Go_Back();stop2=1;	}	
																																	
									}
						 else 
									{
										if(stop2==1)
										{Stop();HAL_Delay(500);
										 stop2=0;
										}
										if(a==0)  //* xe quay trái
										{
											if(cont==1)
												{stop1=1;Go_Straight();if(count_time_delay>250){Stop();HAL_Delay(500);cont++;left=1;count_time_delay=0;}}	//* xe di thang sau khi quay trai lan 1
											else if(left==1&&cont<3)Deg_90_left=1;		//*xe quay trái 90 lan 1 & 3
											else if (cont>=3)
											{cont=0;a=1-a;stop1=1;}					//*  sau khi quay xong 2 lan, bat co` trai phai
											else if (Deg_90_left==0)Go_Straight();		//* cho xe di thang sau khi hoan thanh sau 2 lan quay
											
										}
										else 		//* xe quay phai
										{
											if(cont==1)
												{stop1=1;Go_Straight();if(count_time_delay>250){Stop();HAL_Delay(500);cont++;right=1;count_time_delay=0;}}
											else if(right==1&&cont<3)Deg_90_right=1;
											else if (cont>=3)
											{cont=0;a=1-a;stop1=1;}
											else if (Deg_90_right==0)Go_Straight();
										}	
									}	
}
	
void HAL_ADC_ConvCpltCallback(ADC_HandleTypeDef* hadc)
{
	if(hadc->Instance == hadc1.Instance)
	{
			adc_value = HAL_ADC_GetValue(hadc);
	}
}

void Go_Straight (void)
{
	//motor_straight=1;motor_back=0;motor_left=0;motor_right=0;
	motor_straight=1;
}
void Go_Back (void)
{
		//motor_straight=0;motor_back=1;motor_left=0;motor_right=0;
	motor_back=1;
}
void Go_Left (void)
{
		//motor_straight=0;motor_back=0;motor_left=1;motor_right=0;
	duty3=125;
}
void Go_Right (void)
{ 
			//motor_straight=0;motor_back=0;motor_left=0;motor_right=1;
	duty1=125;
}
void Stop(void)
{
	motor_straight=0;motor_back=0;motor_left=0;motor_right=0;
	Deg_90_left=0;Deg_90_right=0;
	duty0=0;duty1=0;duty2=0;duty3=0;	
	Output=0;Output1=0;Pulse=0;Pulse1=0;
	pre_pre_Err= 0;pre_Err=0; 
	pre_pre_Err1= 0;pre_Err1=0;
	pre_pre_Err2= 0;pre_Err2=0;
}

void Motorpid_1(void)
{
rSpeed=(p-pre_Pulse)*3000/400; //tinh van toc (trong sampling time)
pre_Pulse=p;
Err=rSpeed1-rSpeed;
Output = Output+Kp*Err+Ki*Sampling_time*(Err+pre_Err)/(2000)+Kd*(Err-2*pre_Err+pre_pre_Err)*inv_Sampling_time;
if (Output >400) Output=400;
if (Output <=0) Output=0;
if(motor_straight==0 && motor_back==0 && motor_right==0)Output=0;
if(motor_straight==1)
{duty0=Output;duty1=0;}
if(motor_back==1||motor_right==1)
{duty1=Output;duty0=0;}
pre_pre_Err= pre_Err;
pre_Err=Err; 
}

void Motorpid_2(void)
{
rSpeed1=(p1-pre_Pulse1)*3000/400; //tinh van toc (trong sampling time)
pre_Pulse1=p1;
Err1=des_Speed-rSpeed1;
Output1 = Output1+Kp2*Err1+Ki2*Sampling_time*(Err1+pre_Err1)/(2000)+Kd2*(Err1-2*pre_Err1+pre_pre_Err1)*inv_Sampling_time;
if (Output1 >400) Output1=400;
if (Output1 <=0) Output1=0;
if(motor_straight==0 && motor_back==0&&motor_left==0)Output1=0;
if(motor_straight==1)
{duty2=Output1;duty3=0;}
if(motor_back==1||motor_left==1)
{duty3=Output1;duty2=0;}
pre_pre_Err1= pre_Err1;
pre_Err1=Err1; 
}


void Motor_Left_Position_PID(void)
{
Err2=des_Position-Pulse1;
if(Err2<0)
{
Err2=-Err2;
pros2=1;
loop=1;Output2=100;
}
else if(Err2>-3&&loop==1)
{pros2=0;Deg_90_left=0;cont++;
duty2=0;left=0;loop=0;}
Output2 = Output2+Kp_Pos*Err2+Ki_Pos*Sampling_time*(Err2+pre_Err2)/(2000)+Kd_Pos*(Err2-2*pre_Err2+pre_pre_Err2)*inv_Sampling_time;
if (Output2 >160) Output2=160;
if (Output2 <=0) Output2=0;
if(Deg_90_left==1&&pros2==0)
{duty3=Output2;duty2=0;}
else if(Deg_90_left==1&&pros2==1)
{duty3=0;duty2=Output2;}
if(Deg_90_left==0)
{Output2=0;Pulse1=0;}
pre_pre_Err2= pre_Err2;
pre_Err2=Err2; 
}

void Motor_Right_Position_PID(void)
{
Err3=des_Position-Pulse;
if(Err3<0)
{
Err3=-Err3;
pros3=1;
loop1=1;Output3=90;
}
else if(Err3>-3&&loop1==1)
{pros3=0;Deg_90_right=0;cont++;
duty0=0;right=0;loop1=0;}
Output3 = Output3+Kp_Pos*Err2+Ki_Pos*Sampling_time*(Err2+pre_Err2)/(2000)+Kd_Pos*(Err2-2*pre_Err2+pre_pre_Err2)*inv_Sampling_time;
if (Output3 >140) Output3=140;
if (Output3 <=0) Output3=0;
if(Deg_90_right==1&&pros3==0)
{duty1=Output3;duty0=0;}
else if(Deg_90_right==1&&pros3==1)
{duty1=0;duty0=Output3;}
if(Deg_90_right==0)
{Output3=0;Pulse=0;}
pre_pre_Err3= pre_Err3;
pre_Err3=Err3; 
}
void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance==htim2.Instance)
	{	
		count_time_ADC++;
		if(cont==1)count_time_delay++;
		C=3000*pulse/400;
		D=3000*pulse1/400;
		if(motor_straight==1||motor_back==1||motor_left==1||motor_right==1)
		{
		Motorpid_1();	
		Motorpid_2();}
		else
		{
		Motor_Left_Position_PID();
		Motor_Right_Position_PID();
		}
		pulse=0;pulse1=0;
		Duty_Config();	
	
	}
}

void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
		if(GPIO_Pin==GPIO_PIN_5)
		{
			if(motor_back==1||motor_straight==1){pulse++;p++;}
			if(Deg_90_right==1&&HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_11) == 1)Pulse++;
			if(Deg_90_right==1&&HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_11) == 0)Pulse--;
					//while(HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_0));
		}
		if(GPIO_Pin==GPIO_PIN_4)
		{
			if(motor_back==1||motor_straight==1){pulse1++;p1++;}
			if(Deg_90_left==1&&HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_12) == 0)Pulse1++;
			if(Deg_90_left==1&&HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_12) == 1)Pulse1--;
		}
		
				if(GPIO_Pin==GPIO_PIN_0)
		{
			Deg_90_right=1;
		}		

}


void Duty_Config(void)
{			__HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_1,duty0);		//motor driver
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_2,duty1);		//motor driver
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_3,duty2);		//motor driver
      __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_4,duty3);		//motor driver
	
			__HAL_TIM_SetCompare(&htim3,TIM_CHANNEL_1,duty4);		//motor clean
			__HAL_TIM_SetCompare(&htim3,TIM_CHANNEL_2,duty5);	
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
  hadc1.Init.ContinuousConvMode = DISABLE;
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
  sConfig.SamplingTime = ADC_SAMPLETIME_480CYCLES;
  HAL_ADC_ConfigChannel(&hadc1, &sConfig);

}

/* I2C1 init function */
void MX_I2C1_Init(void)
{

  hi2c1.Instance = I2C1;
  hi2c1.Init.ClockSpeed = 100000;
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
  htim2.Init.Period = 39;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  HAL_TIM_Base_Init(&htim2);

  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  HAL_TIM_ConfigClockSource(&htim2, &sClockSourceConfig);

  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig);

}

/* TIM3 init function */
void MX_TIM3_Init(void)
{

  TIM_ClockConfigTypeDef sClockSourceConfig;
  TIM_MasterConfigTypeDef sMasterConfig;
  TIM_OC_InitTypeDef sConfigOC;

  htim3.Instance = TIM3;
  htim3.Init.Prescaler = 21;
  htim3.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim3.Init.Period = 400;
  htim3.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  HAL_TIM_Base_Init(&htim3);

  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  HAL_TIM_ConfigClockSource(&htim3, &sClockSourceConfig);

  HAL_TIM_PWM_Init(&htim3);

  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  HAL_TIMEx_MasterConfigSynchronization(&htim3, &sMasterConfig);

  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  HAL_TIM_PWM_ConfigChannel(&htim3, &sConfigOC, TIM_CHANNEL_1);

  HAL_TIM_PWM_ConfigChannel(&htim3, &sConfigOC, TIM_CHANNEL_2);

  HAL_TIM_MspPostInit(&htim3);

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

  HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_4);

  HAL_TIM_MspPostInit(&htim4);

}

/* USART2 init function */
void MX_USART2_UART_Init(void)
{

  huart2.Instance = USART2;
  huart2.Init.BaudRate = 9600;
  huart2.Init.WordLength = UART_WORDLENGTH_8B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_NONE;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_16;
  HAL_UART_Init(&huart2);

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

  /*Configure GPIO pins : PA4 PA5 */
  GPIO_InitStruct.Pin = GPIO_PIN_4|GPIO_PIN_5;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA6 PA8 PA9 */
  GPIO_InitStruct.Pin = GPIO_PIN_6|GPIO_PIN_8|GPIO_PIN_9;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA7 PA10 PA15 */
  GPIO_InitStruct.Pin = GPIO_PIN_7|GPIO_PIN_10|GPIO_PIN_15;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PB0 PB1 */
  GPIO_InitStruct.Pin = GPIO_PIN_0|GPIO_PIN_1;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

	GPIO_InitStruct.Pin = GPIO_PIN_11|GPIO_PIN_12;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
	
  /*Configure GPIO pin : PD9 */
  GPIO_InitStruct.Pin = GPIO_PIN_9;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOD, &GPIO_InitStruct);

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_7|GPIO_PIN_10|GPIO_PIN_15, GPIO_PIN_RESET);

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

<template>
	<view class="page" v-if="bannerObj['10002']">
		<image class="bg" :src="bannerObj['10002'].imgUrl" mode="widthFix"></image>
		<view class="box">
			<up-navbar title="花店端" bgColor='#ffffff00' title-color="#000" leftIcon='' :fixed="false" placeholder />
			<view class="userInfo">
				<image class="avatar" :src="shop.businessImg || '/static/logo.png'" mode="aspectFill"></image>
				<view class="right">
					<view class="name">
						<view class="text">{{ shop.name }}</view>
					</view>
					<view class="info">
						<view class="phone">
							{{ shop.mobile }}
						</view>
					</view>
				</view>
			</view>
			<view class="square" style="margin: 24rpx 0;">
				<view class="servceList2">
					<view class="item" @click="toPerson" style="padding-top: 0;">
						<view class="left">
							<image src="/static/icon01.png" mode=""></image>
							<view class="text" style="font-weight: 600;font-size: 32rpx;">回到个人端</view>
						</view>
						<up-icon name="arrow-right"></up-icon>
					</view>
				</view>
			</view>
			<view class="square">
				<view class="title" @click="href('/pages2ShopPort/order/index?index=0', true)">
					<view class="name">花店订单</view>
					<view class="right">
						<up-icon name="arrow-right" color="#333"></up-icon>
					</view>
				</view>
				<view class="orderList">
					<view class="item" v-for="item, index in orderList" :key="index"
						@click="href('/pages2ShopPort/order/index?index=' + item[2], true)">
						<up-badge :offset="[0, 10]" :max="99" :absolute='true' :value="orderCount[index]"></up-badge>
						<image :src="item[0]" mode="widthFix"></image>
						<view class="name">{{ item[1] }}</view>
					</view>
				</view>
			</view>
			<view class="square">
				<view class="title">
					<view class="name">服务中心</view>
				</view>
				<view class="servceList">
					<view v-for="item, index in servceList" :key="index" @click="to(item, index)">
						<view class="item" v-if="item[1] != '客服中心'">
							<image :src="item[0]" mode=""></image>
							<view class="text">{{ item[1] }}</view>
						</view>
						<button open-type="contact" v-else>
							<view class="item">
								<image :src="item[0]" mode=""></image>
								<view class="text">{{ item[1] }}</view>
							</view>
						</button>
					</view>
				</view>
				<!-- 	<view class="servceList2">
					<view v-for="item,index in servceList" :key="index" @click="to(item,index)">
						<view class="item" v-if="item[1]!='客服中心'">
							<view class="left">
								<image :src="item[0]" mode=""></image>
								<view class="text">{{item[1]}}</view>
							</view>
							<up-icon name="arrow-right"></up-icon>
						</view>
						<button open-type="contact" v-else>
							<view class="item">
								<view class="left">
									<image :src="item[0]" mode=""></image>
									<view class="text">{{item[1]}}</view>
								</view>
								<up-icon name="arrow-right"></up-icon>
							</view>
						</button>
					</view>
				</view> -->
			</view>
			<view style="height: 32rpx;"></view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			shop:{},
			bannerObj: {},
			// 我的订单
			orderList: [
				['/static/icon08.png', '已发布', 3],
				['/static/icon09.png', '进行中', 4],
				['/static/icon10.png', '待收货', 5],
				['/static/icon11.png', '已完成', 6],
				['/static/icon12.png', '售后', 7],
			],
			// 服务中心
			servceList: [
				['/static/icon22.png', "发布提示", ""],
				['/static/icon13.png', "发布任务", "/pagesShop/task/add"],
			],
			orderCount: [],
			pointOut: ''
		};
	},
	onLoad() {
		// 背景图  
		this.request(`news/BannerList`, {
			Ids: [10002]
		}).then(res => {
			this.bannerObj = res.data.items
		})
		this.request('Other/GetConfig').then(res => {
			this.pointOut = res.data.textContents
		})
		this.request('User/GetShop').then(res => {
		    if (res.code == 200) {
				this.shop =  res.data.shop
		    }
		})
	},
	onShow() {
		this.storeUser.updateUser()
		if (this.getToken()) {
			this.getOrderCount()
		}
	},
	methods: {
		async getOrderCount() {
			let res = await this.request('Order/OrderCount')
			let arr = Object.values(res.data)
			this.orderCount = arr
		},
		to(item, index) {
			if (item[1] == '客服中心') {

			} else if (item[1] == '发布提示') {
				this.modal('任务发布提示', this.pointOut)
			} else {
				this.href(item[2], true)
			}
		},
		toMember() {
			uni.switchTab({
				url: '/pages/member/index'
			})
		},
		toPerson() {
			uni.reLaunch({
				url: '/pages/person/index'
			})
		}
	}
}
</script>

<style lang="scss" scoped>
button {
	padding: 0;
	background-color: initial;
	margin: 0;
	line-height: initial;
	overflow: initial;
	box-sizing: initial;
	font-size: initial;
	border: 0;

}

button::after {
	content: none;
}

.page {
	min-height: 100vh;
	width: 750rpx;
}

.bg {
	width: 750rpx;
}

.box {
	position: absolute;
	width: 100%;
	left: 0;
	top: 0;
	padding: 0 40rpx;
}

.userInfo {
	margin: 32rpx 0;
	display: flex;
	align-items: center;

	.avatar {
		width: 112rpx;
		height: 112rpx;
		border-radius: 50%;
	}

	.right {
		margin-left: 24rpx;
		flex: 1;

		.name {
			font-weight: 500;
			font-size: 36rpx;
			color: #333333;
			display: flex;
			align-items: center;

			image {
				margin-left: 1rpx;
				width: 40rpx;
				height: 40rpx;
			}
		}

		.info {
			margin-top: 12rpx;
			display: flex;
			align-items: center;

			.star {
				display: flex;
				align-items: center;
				background: linear-gradient(270deg, #E5BE7D 0%, #E1A770 100%);
				border-radius: 28rpx;
				padding: 6rpx 16rpx;

				.text {
					margin-left: 2rpx;
					font-size: 20rpx;
					color: #FFFFFF;
				}
			}

			.phone {
				// margin-left: 16rpx;
				font-size: 24rpx;
				color: #999999;
			}
		}

		.LoginTitle {
			display: flex;
			height: 100%;
			align-items: center;
			color: #8C4FFF;
			font-size: 34rpx;
		}

	}
}

.square {
	margin-bottom: 24rpx;
	padding: 24rpx;
	border-radius: 16rpx;
	background: white;

	.title {
		display: flex;
		justify-content: space-between;
		background: white;

		.name {
			font-weight: 600;
			font-size: 28rpx;
			color: #333333;
		}
	}

	.orderList {
		padding-top: 28rpx;
		display: flex;

		.item {
			width: 25%;
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
			position: relative;

			image {
				width: 56rpx;
				height: 56rpx;
			}

			.name {
				font-size: 28rpx;
				color: #000000;
				margin-top: 4rpx;
			}
		}
	}

	.servceList {
		display: flex;
		flex-wrap: wrap;

		>view {
			width: 25%;
			margin-top: 32rpx;

			.item {
				width: 100%;
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;

				image {
					width: 48rpx;
					height: 48rpx;
				}

				.text {
					margin-top: 16rpx;
					font-size: 30rpx;
					color: #333333;
				}
			}
		}
	}

	.servceList2 {
		.item {
			padding-top: 32rpx;
			display: flex;
			align-items: center;
			justify-content: space-between;

			.left {
				display: flex;
				align-items: center;

				image {
					width: 56rpx;
					height: 56rpx;
				}

				.text {
					font-size: 24rpx;
					color: #333333;
					margin-left: 16rpx;
				}
			}
		}
	}

}
</style>